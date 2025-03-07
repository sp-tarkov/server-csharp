using System.Text.RegularExpressions;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Inventory;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Services;

[Injectable]
public class MapMarkerService(
    ISptLogger<MapMarkerService> _logger
)
{
    /// <summary>
    ///     Add note to a map item in player inventory
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Add marker request</param>
    /// <returns>Item</returns>
    public Item CreateMarkerOnMap(PmcData pmcData, InventoryCreateMarkerRequestData request)
    {
        // Get map from inventory
        var mapItem = pmcData?.Inventory?.Items?.FirstOrDefault(i => i?.Id == request?.Item);

        // add marker to map item
        mapItem.Upd.Map = mapItem?.Upd?.Map ??
                          new UpdMap
                          {
                              Markers = new List<MapMarker>()
                          };

        // Update request note with text, then add to maps upd
        request.MapMarker.Note = SanitiseMapMarkerText(request.MapMarker.Note);
        mapItem?.Upd?.Map?.Markers?.Add(request.MapMarker);

        return mapItem;
    }

    /// <summary>
    ///     Delete a map marker
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Delete marker request</param>
    /// <returns>Item</returns>
    public Item DeleteMarkerFromMap(PmcData pmcData, InventoryDeleteMarkerRequestData request)
    {
        // Get map from inventory
        var mapItem = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == request.Item);

        // remove marker
        var markers = mapItem.Upd.Map.Markers.Where(
                marker =>
                {
                    return marker.X != request.X && marker.Y != request.Y;
                }
            )
            .ToList();
        mapItem.Upd.Map.Markers = markers;

        return mapItem;
    }

    /// <summary>
    ///     Edit an existing map marker
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Edit marker request</param>
    /// <returns>Item</returns>
    public Item? EditMarkerOnMap(PmcData pmcData, InventoryEditMarkerRequestData request)
    {
        // Get map from inventory
        var mapItem = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == request.Item);

        // edit marker
        // the only thing that is consistent between the old and edit is the X and Y
        // find the marker where X and Y match
        var markerToRemove = mapItem.Upd.Map.Markers.FirstOrDefault(x => x.X == request.X && x.Y == request.Y);

        if (markerToRemove is null)
        {
            _logger.Warning($"No marker found for item {request.Item}");
            return null;
        }

        request.MapMarker.Note = SanitiseMapMarkerText(request.MapMarker.Note);
        mapItem.Upd.Map.Markers.Remove(markerToRemove);
        mapItem.Upd.Map.Markers.Add(request.MapMarker);

        return mapItem;
    }

    /// <summary>
    ///     Strip out characters from note string that are not: letter/numbers/unicode/spaces
    /// </summary>
    /// <param name="mapNoteText">Marker text to sanitise</param>
    /// <returns>Sanitised map marker text</returns>
    protected string SanitiseMapMarkerText(string mapNoteText)
    {
        return Regex.Replace(mapNoteText, @"[^\p{L}\d\s]", "", RegexOptions.CultureInvariant);
    }
}
