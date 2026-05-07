using System.Collections.Concurrent;
using System.Reflection;
using NUnit.Framework;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Ragfair;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace UnitTests.Tests.Utils;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public sealed class RagfairHolderTests
{
    private const int SeedOfferCount = 25000;
    private const int AdditionalOfferCount = 10000;
    private const int TemplateCount = 250;
    private const int TraderCount = 100;

    private static readonly MongoId[] TemplateIds = Enumerable.Range(0, TemplateCount).Select(_ => new MongoId()).ToArray();

    private static readonly MongoId[] TraderIds = Enumerable.Range(0, TraderCount).Select(_ => new MongoId()).ToArray();

    private RagfairOfferHolder _ragfairOfferHolder = default!;

    [OneTimeSetUp]
    public void Initialize()
    {
        _ragfairOfferHolder = DI.GetInstance().GetService<RagfairOfferHolder>();
    }

    [SetUp]
    public void Reset()
    {
        ResetHolderState(_ragfairOfferHolder);
    }

    [Test]
    [Timeout(60_000)]
    public void AddOffers_LargeBatch_ShouldStoreAllOffers()
    {
        var holder = CreateHolder();
        var offers = CreateOffers(SeedOfferCount).ToList();

        holder.AddOffers(offers);

        var storedOffers = holder.GetOffers();

        Assert.That(storedOffers, Has.Count.EqualTo(SeedOfferCount));
    }

    [Test]
    [Timeout(60_000)]
    public void AddOffers_LargeBatch_ShouldBeQueryableByIdTemplateAndTrader()
    {
        var holder = CreateHolder();
        var offers = CreateOffers(SeedOfferCount).ToList();

        holder.AddOffers(offers);

        foreach (var offer in offers.Take(500))
        {
            var storedOffer = holder.GetOfferById(offer.Id);

            Assert.That(storedOffer, Is.Not.Null);
            Assert.That(storedOffer!.Id, Is.EqualTo(offer.Id));

            var rootItem = offer.Items.FirstOrDefault();

            Assert.That(rootItem, Is.Not.Null);

            var offersByTemplate = ToListOrEmpty(holder.GetOffersByTemplate(rootItem!.Template));

            Assert.That(
                offersByTemplate.Any(indexedOffer => indexedOffer.Id == offer.Id),
                Is.True,
                $"Offer {offer.Id} was not found by template {rootItem.Template}"
            );

            if (offer.IsTraderOffer())
            {
                var offersByTrader = holder.GetOffersByTrader(offer.User.Id).ToList();

                Assert.That(
                    offersByTrader.Any(indexedOffer => indexedOffer.Id == offer.Id),
                    Is.True,
                    $"Offer {offer.Id} was not found by trader {offer.User.Id}"
                );
            }
        }
    }

    [Test]
    [Timeout(60_000)]
    public async Task ConcurrentLargeBatchOperations_ShouldCompleteWithoutDeadlockOrUnhandledExceptions()
    {
        var holder = CreateHolder();
        var seedOffers = CreateOffers(SeedOfferCount).ToList();

        holder.AddOffers(seedOffers);

        var errors = new ConcurrentQueue<Exception>();
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        var seedOfferIds = seedOffers.Select(offer => offer.Id).ToArray();
        var seedTemplates = seedOffers.Select(offer => offer.Items.First().Template).Distinct().ToArray();
        var seedTraderIds = seedOffers.Where(offer => offer.IsTraderOffer()).Select(offer => offer.User.Id).Distinct().ToArray();

        var tasks = new List<Task>
        {
            CaptureErrorsAsync(errors, cancellationTokenSource.Token, token => AddOffersUntilCancelled(holder, token)),
            CaptureErrorsAsync(
                errors,
                cancellationTokenSource.Token,
                token => AddManyOffersUntilCancelled(holder, AdditionalOfferCount, token)
            ),
            CaptureErrorsAsync(errors, cancellationTokenSource.Token, token => RemoveOffersUntilCancelled(holder, seedOfferIds, token)),
            CaptureErrorsAsync(errors, cancellationTokenSource.Token, token => ReadByIdUntilCancelled(holder, seedOfferIds, token)),
            CaptureErrorsAsync(errors, cancellationTokenSource.Token, token => ReadByTemplateUntilCancelled(holder, seedTemplates, token)),
            CaptureErrorsAsync(errors, cancellationTokenSource.Token, token => ReadByTraderUntilCancelled(holder, seedTraderIds, token)),
            CaptureErrorsAsync(errors, cancellationTokenSource.Token, token => ExpireOffersUntilCancelled(holder, token)),
            CaptureErrorsAsync(errors, cancellationTokenSource.Token, token => ReadExpiredOffersUntilCancelled(holder, token)),
        };

        await Task.WhenAll(tasks);

        if (!errors.IsEmpty)
        {
            throw new AggregateException(errors);
        }
    }

    [Test]
    [Timeout(60_000)]
    public async Task ConcurrentReadsWhileRemoving_ShouldNotThrowCollectionModifiedExceptions()
    {
        var holder = CreateHolder();
        var offers = CreateOffers(SeedOfferCount).ToList();

        holder.AddOffers(offers);

        var errors = new ConcurrentQueue<Exception>();
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        var offerIds = offers.Select(offer => offer.Id).ToArray();
        var templates = offers.Select(offer => offer.Items.First().Template).Distinct().ToArray();
        var traderIds = offers.Where(offer => offer.IsTraderOffer()).Select(offer => offer.User.Id).Distinct().ToArray();

        var tasks = new List<Task>
        {
            CaptureErrorsAsync(errors, cancellationTokenSource.Token, token => RemoveOffersUntilCancelled(holder, offerIds, token)),
            CaptureErrorsAsync(errors, cancellationTokenSource.Token, token => RemoveOffersUntilCancelled(holder, offerIds, token)),
            CaptureErrorsAsync(errors, cancellationTokenSource.Token, token => RemoveOffersUntilCancelled(holder, offerIds, token)),
            CaptureErrorsAsync(errors, cancellationTokenSource.Token, token => ReadByTemplateUntilCancelled(holder, templates, token)),
            CaptureErrorsAsync(errors, cancellationTokenSource.Token, token => ReadByTemplateUntilCancelled(holder, templates, token)),
            CaptureErrorsAsync(errors, cancellationTokenSource.Token, token => ReadByTraderUntilCancelled(holder, traderIds, token)),
            CaptureErrorsAsync(errors, cancellationTokenSource.Token, token => ReadByTraderUntilCancelled(holder, traderIds, token)),
            CaptureErrorsAsync(errors, cancellationTokenSource.Token, token => ReadAllOffersUntilCancelled(holder, token)),
        };

        await Task.WhenAll(tasks);

        if (!errors.IsEmpty)
        {
            throw new AggregateException(errors);
        }
    }

    [Test]
    [Timeout(60_000)]
    public void RemoveAllOffersByTrader_LargeBatch_ShouldRemoveTraderOffers()
    {
        var holder = CreateHolder();
        var traderId = new MongoId();
        var traderOffers = CreateTraderOffers(10_000, traderId).ToList();

        holder.AddOffers(traderOffers);

        Assert.That(holder.GetOffersByTrader(traderId).Count(), Is.EqualTo(traderOffers.Count));

        holder.RemoveAllOffersByTrader(traderId);

        var offersByTrader = holder.GetOffersByTrader(traderId).ToList();

        Assert.That(offersByTrader, Is.Empty);

        foreach (var offer in traderOffers.Take(500))
        {
            var storedOffer = holder.GetOfferById(offer.Id);

            Assert.That(storedOffer, Is.Null);
        }
    }

    [Test]
    [Timeout(60_000)]
    public void RemoveAllOffersByTrader_LargeBatch_ShouldNotLeaveOfferIdsInTemplateIndex()
    {
        var holder = CreateHolder();
        var traderId = new MongoId();
        var traderOffers = CreateTraderOffers(10_000, traderId).ToList();
        var traderOfferIds = traderOffers.Select(offer => offer.Id).ToHashSet();

        holder.AddOffers(traderOffers);
        holder.RemoveAllOffersByTrader(traderId);

        var templateIndexValues = GetPrivateIndexValues(holder, "_offersByTemplate");
        var leakedOfferIds = templateIndexValues.SelectMany(offerIds => offerIds).Where(traderOfferIds.Contains).Take(20).ToList();

        Assert.That(
            leakedOfferIds,
            Is.Empty,
            "RemoveAllOffersByTrader removed offers from _offersById, but stale offer ids still exist in _offersByTemplate."
        );
    }

    [Test]
    [Timeout(60_000)]
    public void RemoveOffer_ShouldRemoveOfferFromAllIndexes()
    {
        var holder = CreateHolder();
        var offer = CreateTraderOffer(0, new MongoId());
        var rootItem = offer.Items.First();

        holder.AddOffer(offer);
        holder.RemoveOffer(offer.Id);

        var storedOffer = holder.GetOfferById(offer.Id);
        var offersByTrader = holder.GetOffersByTrader(offer.User.Id).ToList();
        var offersByTemplate = ToListOrEmpty(holder.GetOffersByTemplate(rootItem.Template));

        Assert.That(storedOffer, Is.Null);
        Assert.That(offersByTrader.Any(indexedOffer => indexedOffer.Id == offer.Id), Is.False);
        Assert.That(offersByTemplate.Any(indexedOffer => indexedOffer.Id == offer.Id), Is.False);
    }

    [Test]
    [Timeout(60_000)]
    public void FlagExpiredOffersAfterDate_LargeBatch_ShouldNotHang()
    {
        var holder = CreateHolder();
        var offers = CreateOffers(SeedOfferCount).ToList();

        holder.AddOffers(offers);

        var timestampFarInFuture = DateTimeOffset.UtcNow.AddYears(10).ToUnixTimeSeconds();

        holder.FlagExpiredOffersAfterDate(timestampFarInFuture);

        var expiredOfferCount = holder.GetExpiredOfferCount();

        Assert.That(expiredOfferCount, Is.GreaterThanOrEqualTo(0));
    }

    private RagfairOfferHolder CreateHolder()
    {
        ResetHolderState(_ragfairOfferHolder);

        return _ragfairOfferHolder;
    }

    private static Task CaptureErrorsAsync(
        ConcurrentQueue<Exception> errors,
        CancellationToken cancellationToken,
        Action<CancellationToken> action
    )
    {
        return Task.Run(
            () =>
            {
                try
                {
                    action(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested.
                }
                catch (Exception exception)
                {
                    errors.Enqueue(exception);
                }
            },
            cancellationToken
        );
    }

    private static void AddOffersUntilCancelled(RagfairOfferHolder holder, CancellationToken cancellationToken)
    {
        var index = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            holder.AddOffer(CreateOffer(SeedOfferCount + AdditionalOfferCount + index));
            index++;
        }
    }

    private static void AddManyOffersUntilCancelled(RagfairOfferHolder holder, int maximumCount, CancellationToken cancellationToken)
    {
        for (var index = 0; index < maximumCount; index++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var offer = CreateOffer(SeedOfferCount + index);

            holder.AddOffer(offer);
        }
    }

    private static void RemoveOffersUntilCancelled(
        RagfairOfferHolder holder,
        IReadOnlyList<MongoId> offerIds,
        CancellationToken cancellationToken
    )
    {
        var index = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            holder.RemoveOffer(offerIds[index % offerIds.Count]);
            index++;
        }
    }

    private static void ReadByIdUntilCancelled(
        RagfairOfferHolder holder,
        IReadOnlyList<MongoId> offerIds,
        CancellationToken cancellationToken
    )
    {
        var index = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var offerId = offerIds[index % offerIds.Count];

            _ = holder.GetOfferById(offerId);
            index++;
        }
    }

    private static void ReadByTemplateUntilCancelled(
        RagfairOfferHolder holder,
        IReadOnlyList<MongoId> templates,
        CancellationToken cancellationToken
    )
    {
        var index = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var template = templates[index % templates.Count];
            var offers = ToListOrEmpty(holder.GetOffersByTemplate(template));

            _ = offers.Count;
            index++;
        }
    }

    private static void ReadByTraderUntilCancelled(
        RagfairOfferHolder holder,
        IReadOnlyList<MongoId> traderIds,
        CancellationToken cancellationToken
    )
    {
        if (traderIds.Count == 0)
        {
            return;
        }

        var index = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var traderId = traderIds[index % traderIds.Count];
            var offers = holder.GetOffersByTrader(traderId).ToList();

            _ = offers.Count;
            index++;
        }
    }

    private static void ReadAllOffersUntilCancelled(RagfairOfferHolder holder, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var offers = holder.GetOffers();

            _ = offers.Count;
        }
    }

    private static void ExpireOffersUntilCancelled(RagfairOfferHolder holder, CancellationToken cancellationToken)
    {
        var timestampFarInFuture = DateTimeOffset.UtcNow.AddYears(10).ToUnixTimeSeconds();

        while (!cancellationToken.IsCancellationRequested)
        {
            holder.FlagExpiredOffersAfterDate(timestampFarInFuture);
        }
    }

    private static void ReadExpiredOffersUntilCancelled(RagfairOfferHolder holder, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _ = holder.GetStaleOfferIds();
            _ = holder.GetExpiredOfferCount();
            _ = holder.GetExpiredOfferItems().ToList();
        }
    }

    private static List<RagfairOffer> ToListOrEmpty(IEnumerable<RagfairOffer>? offers)
    {
        if (offers is null)
        {
            return [];
        }

        return offers.ToList();
    }

    private static void ResetHolderState(RagfairOfferHolder holder)
    {
        ClearPrivateCollection(holder, "_expiredOfferIds");
        ClearPrivateCollection(holder, "_offersById");
        ClearPrivateCollection(holder, "_offersByTemplate");
        ClearPrivateCollection(holder, "_offersByTrader");
        ClearPrivateCollection(holder, "_fakePlayerOffers");
    }

    private static void ClearPrivateCollection(RagfairOfferHolder holder, string fieldName)
    {
        var field = typeof(RagfairOfferHolder).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null, $"Could not find private field {fieldName}");

        var value = field!.GetValue(holder);

        Assert.That(value, Is.Not.Null, $"Private field {fieldName} was null");

        var clearMethod = value!.GetType().GetMethod("Clear", Type.EmptyTypes);

        Assert.That(clearMethod, Is.Not.Null, $"Private field {fieldName} does not have a Clear method");

        clearMethod!.Invoke(value, null);
    }

    private static IEnumerable<HashSet<MongoId>> GetPrivateIndexValues(RagfairOfferHolder holder, string fieldName)
    {
        var field = typeof(RagfairOfferHolder).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null, $"Could not find private field {fieldName}");

        var value = field!.GetValue(holder);

        Assert.That(value, Is.Not.Null, $"Private field {fieldName} was null");

        if (value is ConcurrentDictionary<MongoId, HashSet<MongoId>> concurrentDictionary)
        {
            return concurrentDictionary.Values.ToList();
        }

        if (value is Dictionary<MongoId, HashSet<MongoId>> dictionary)
        {
            return dictionary.Values.ToList();
        }

        Assert.Fail($"Private field {fieldName} was not a supported index type.");

        return [];
    }

    private static IEnumerable<RagfairOffer> CreateOffers(int count)
    {
        for (var index = 0; index < count; index++)
        {
            yield return CreateOffer(index);
        }
    }

    private static IEnumerable<RagfairOffer> CreateTraderOffers(int count, MongoId traderId)
    {
        for (var index = 0; index < count; index++)
        {
            yield return CreateTraderOffer(index, traderId);
        }
    }

    private static RagfairOffer CreateOffer(int index)
    {
        var templateId = TemplateIds[index % TemplateIds.Length];

        if (index % 5 == 0)
        {
            var traderId = TraderIds[index % TraderIds.Length];

            return CreateTraderOffer(index, traderId, templateId);
        }

        return CreatePlayerOffer(index, templateId);
    }

    private static RagfairOffer CreatePlayerOffer(int index, MongoId templateId)
    {
        var offerId = new MongoId();
        var userId = new MongoId();
        var rootItemId = new MongoId();
        var rootItem = CreateRootItem(rootItemId, templateId);

        return new RagfairOffer
        {
            Id = offerId,
            InternalId = index,
            CreatedBy = OfferCreator.Player,
            User = new RagfairOfferUser
            {
                Id = userId,
                MemberType = MemberCategory.Default,
                Nickname = $"StressPlayer_{index}",
                Rating = 0,
                IsRatingGrowing = false,
                Avatar = null,
                Aid = index,
            },
            Root = rootItemId,
            Items = [rootItem],
            ItemsCost = 1,
            Requirements =
            [
                new OfferRequirement
                {
                    TemplateId = Money.ROUBLES,
                    Count = 1000,
                    OnlyFunctional = false,
                },
            ],
            RequirementsCost = 1000,
            SummaryCost = 1000,
            StartTime = 1,
            EndTime = 2,
            LoyaltyLevel = 1,
            SellInOnePiece = false,
            Locked = false,
            Quantity = 1,
        };
    }

    private static RagfairOffer CreateTraderOffer(int index, MongoId traderId)
    {
        var templateId = TemplateIds[index % TemplateIds.Length];

        return CreateTraderOffer(index, traderId, templateId);
    }

    private static RagfairOffer CreateTraderOffer(int index, MongoId traderId, MongoId templateId)
    {
        var offerId = new MongoId();
        var rootItemId = new MongoId();
        var rootItem = CreateRootItem(rootItemId, templateId);

        return new RagfairOffer
        {
            Id = offerId,
            InternalId = index,
            CreatedBy = OfferCreator.Trader,
            User = new RagfairOfferUser
            {
                Id = traderId,
                MemberType = MemberCategory.Trader,
                Nickname = $"StressTrader_{traderId}",
                Rating = 1,
                IsRatingGrowing = false,
                Avatar = null,
                Aid = index,
            },
            Root = rootItemId,
            Items = [rootItem],
            ItemsCost = 1,
            Requirements =
            [
                new OfferRequirement
                {
                    TemplateId = Money.ROUBLES,
                    Count = 1000,
                    OnlyFunctional = false,
                },
            ],
            RequirementsCost = 1000,
            SummaryCost = 1000,
            StartTime = 1,
            EndTime = long.MaxValue,
            LoyaltyLevel = 1,
            SellInOnePiece = false,
            Locked = false,
            Quantity = 1,
        };
    }

    private static Item CreateRootItem(MongoId rootItemId, MongoId templateId)
    {
        return new Item
        {
            Id = rootItemId,
            Template = templateId,
            ParentId = null,
            SlotId = null,
            Upd = new Upd { StackObjectsCount = 1 },
        };
    }
}
