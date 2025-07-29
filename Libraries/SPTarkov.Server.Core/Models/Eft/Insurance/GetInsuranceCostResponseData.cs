using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Insurance;

public class GetInsuranceCostResponseData : Dictionary<MongoId, Dictionary<MongoId, double>> { }
