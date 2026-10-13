using Dan.Common.Exceptions;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Services;
using Dan.Common.Util;
using Dan.Plugin.Ebevis.Config;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Dan.Plugin.Ebevis;

public class Plugin
{
    private readonly IEvidenceSourceMetadata _evidenceSourceMetadata;
    private readonly ILogger _logger;
    private readonly ApplicationSettings _settings;
    private readonly IDanPluginClientService _danPluginClientService;
    private readonly ICcrClientService _ccrClientService;

    private const int ERROR_NOT_FOUND = 1003;
    private const string EBEVIS = "eBevis";

    // Dataset names in this plugin's own metadata, used when building responses
    private const string TradeRegisterEnrolment = "CRITERION.SELECTION.SUITABILITY.TRADE_REGISTER_ENROLMENT";
    private const string TurnoverSetUp = "CRITERION.SELECTION.ECONOMIC_FINANCIAL_STANDING.TURNOVER.SET_UP";
    private const string ManagerialStaff = "CRITERION.SELECTION.TECHNICAL_PROFESSIONAL_ABILITY.MANAGEMENT.MANAGERIAL_STAFF";
    private const string AverageAnnualManpower = "CRITERION.SELECTION.TECHNICAL_PROFESSIONAL_ABILITY.MANAGEMENT.AVERAGE_ANNUAL_MANPOWER";
    private const string BusinessInsolvency = "CRITERION.EXCLUSION.BUSINESS.INSOLVENCY";
    private const string BusinessBankruptcy = "CRITERION.EXCLUSION.BUSINESS.BANKRUPTCY";
    private const string TurnoverGeneralYearly = "CRITERION.SELECTION.ECONOMIC_FINANCIAL_STANDING.TURNOVER.GENERAL_YEARLY";
    private const string BilpleieregisteretEbevisDataset = "BilpleieregisteretEbevis";

    // Plugin sources as used in the DAN function app naming convention (func-es{source}-{env}), see DanPluginClientService
    private const string SourceBr = "brreg";
    private const string SourceSvv = "svv";
    private const string SourceArbt = "arbt";

    // Dataset names in the upstream plugins. These must match the public metadata exactly (case sensitive)
    private const string CertificateOfRegistration = "CertificateOfRegistration";
    private const string AnnualFinancialReport = "AnnualFinancialReport";
    private const string Bilpleieregisteret = "Bilpleieregisteret";
    private const string Verkstedregisteret = "Verkstedregisteret";

    public Plugin(
        ILoggerFactory loggerFactory,
        IOptions<ApplicationSettings> settings,
        IEvidenceSourceMetadata evidenceSourceMetadata,
        IDanPluginClientService danPluginClientService,
        ICcrClientService ccrClientService)
    {
        _logger = loggerFactory.CreateLogger<Plugin>();
        _settings = settings.Value;
        _evidenceSourceMetadata = evidenceSourceMetadata;
        _danPluginClientService = danPluginClientService;
        _ccrClientService = ccrClientService;

        // Synthetic (Tenor) organisation numbers are only looked up in explicitly non-production environments.
        // Fail closed: a missing or unrecognised DanEnvironment must never enable test lookups.
        _ccrClientService.AllowTestCcrLookup =
            string.Equals(_settings.DanEnvironment, "dev", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(_settings.DanEnvironment, "test", StringComparison.OrdinalIgnoreCase);
    }

    [Function("CriterionSelectionSuitabilityTradeRegisterEnrolment")]
    public async Task<HttpResponseData> CriterionSelectionSuitabilityTradeRegisterEnrolment(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = TradeRegisterEnrolment)]
            HttpRequestData req,
            FunctionContext context)
    {
        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();
        return await EvidenceSourceResponse.CreateResponse(req, () => GetCriterionSelectionSuitabilityTradeRegisterEnrolment(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetCriterionSelectionSuitabilityTradeRegisterEnrolment(EvidenceHarvesterRequest req)
    {
        var unit = await GetCcrUnit(req);

        var ecb = new EvidenceBuilder(_evidenceSourceMetadata, TradeRegisterEnrolment);
        ecb.AddEvidenceValue("IsInRegistryOfBusinessEnterprises", unit.RegistrertIForetaksregisteret);
        return ecb.GetEvidenceValues();
    }

    [Function("CriterionSelectionEconomicFinanciaLStandingTurnoverSetUp")]
    public async Task<HttpResponseData> CriterionSelectionEconomicFinanciaLStandingTurnoverSetUp(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = TurnoverSetUp)]
            HttpRequestData req,
        FunctionContext context)
    {
        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();
        return await EvidenceSourceResponse.CreateResponse(req, () => GetCriterionSelectionEconomicFinanciaLStandingTurnoverSetUp(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetCriterionSelectionEconomicFinanciaLStandingTurnoverSetUp(EvidenceHarvesterRequest req)
    {
        var unit = await GetCcrUnit(req);

        var ecb = new EvidenceBuilder(_evidenceSourceMetadata, TurnoverSetUp);
        ecb.AddEvidenceValue("CreatedInCentralRegisterForLegalEntities", unit.RegistreringsdatoEnhetsregisteret?.DateTime);
        ecb.AddEvidenceValue("Established", unit.Stiftelsesdato?.DateTime);
        return ecb.GetEvidenceValues();
    }

    [Function("CriterionSelectionTechnicalProfessionalAbilityManagementManagerialStaff")]
    public async Task<HttpResponseData> CriterionSelectionTechnicalProfessionalAbilityManagementManagerialStaff(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = ManagerialStaff)]
            HttpRequestData req, FunctionContext context)
    {
        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();
        return await EvidenceSourceResponse.CreateResponse(req, () => GetEmployeesAndCertificateOfRegistration(evidenceHarvesterRequest, ManagerialStaff));
    }

    [Function("CriterionSelectionTechnicalProfessionalAbilityManagementAverageAnnualManpower")]
    public async Task<HttpResponseData> CriterionSelectionTechnicalProfessionalAbilityManagementAverageAnnualManpower(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = AverageAnnualManpower)]
            HttpRequestData req, FunctionContext context)
    {
        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();
        return await EvidenceSourceResponse.CreateResponse(req, () => GetEmployeesAndCertificateOfRegistration(evidenceHarvesterRequest, AverageAnnualManpower));
    }

    /// <summary>
    /// Number of employees comes from the CCR lookup, the certificate of registration (firmaattest) is only available from the BR plugin
    /// </summary>
    private async Task<List<EvidenceValue>> GetEmployeesAndCertificateOfRegistration(EvidenceHarvesterRequest req, string datasetName)
    {
        var unitTask = GetCcrUnit(req);
        var certificateOfRegistrationTask = GetDataFromBr(req, CertificateOfRegistration);

        await Task.WhenAll(unitTask, certificateOfRegistrationTask);

        var certificateUrl = certificateOfRegistrationTask.Result.Find(x => x.EvidenceValueName == "CertificateOfRegistrationPdfUrl")?.Value;

        var ecb = new EvidenceBuilder(_evidenceSourceMetadata, datasetName);
        ecb.AddEvidenceValue("NumberOfEmployees", unitTask.Result.AntallAnsatte);
        ecb.AddEvidenceValue("CertificateOfRegistrationPdfUrl", certificateUrl);
        return ecb.GetEvidenceValues();
    }

    [Function("CriterionExclusionBusinessInsolvency")]
    public async Task<HttpResponseData> CriterionExclusionBusinessInsolvency(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = BusinessInsolvency)]
            HttpRequestData req, FunctionContext context)
    {
        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();
        return await EvidenceSourceResponse.CreateResponse(req, () => GetCriterionExclusionBusinessInsolvency(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetCriterionExclusionBusinessInsolvency(EvidenceHarvesterRequest req)
    {
        var unit = await GetCcrUnit(req);

        var ecb = new EvidenceBuilder(_evidenceSourceMetadata, BusinessInsolvency);
        ecb.AddEvidenceValue("IsBeingDissolved", unit.UnderAvvikling);
        ecb.AddEvidenceValue("IsBeingForciblyDissolved", unit.UnderTvangsavviklingEllerTvangsopplosning);
        return ecb.GetEvidenceValues();
    }

    [Function("CriterionExclusionBusinessBankruptcy")]
    public async Task<HttpResponseData> CriterionExclusionBusinessBankruptcy(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = BusinessBankruptcy)]
            HttpRequestData req, FunctionContext context)
    {
        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();
        return await EvidenceSourceResponse.CreateResponse(req, () => GetCriterionExclusionBusinessBankruptcy(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetCriterionExclusionBusinessBankruptcy(EvidenceHarvesterRequest req)
    {
        var unit = await GetCcrUnit(req);

        var ecb = new EvidenceBuilder(_evidenceSourceMetadata, BusinessBankruptcy);
        ecb.AddEvidenceValue("IsUnderBankruptcy", unit.Konkurs);
        return ecb.GetEvidenceValues();
    }

    [Function("CriterionSelectionEconomicFinancialStandingTurnoverGeneralYearly")]
    public async Task<HttpResponseData> CriterionSelectionEconomicFinancialStandingTurnoverGeneralYearly(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = TurnoverGeneralYearly)]
            HttpRequestData req, FunctionContext context)
    {
        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();
        return await EvidenceSourceResponse.CreateResponse(req, () => GetDataFromBr(evidenceHarvesterRequest, AnnualFinancialReport));
    }

    [Function("BilpleieregisteretEbevis")]
    public async Task<HttpResponseData> BilpleieregisteretEbevis(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = BilpleieregisteretEbevisDataset)]
        HttpRequestData req, FunctionContext context)
    {
        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();
        return await EvidenceSourceResponse.CreateResponse(req, () => GetBilpleieData(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetBilpleieData(EvidenceHarvesterRequest evidenceHarvesterRequest)
    {
        var taskList = new List<Task<List<EvidenceValue>>>
        {
            GetDataFromArbt(evidenceHarvesterRequest, Bilpleieregisteret),
            GetDataFromSvv(evidenceHarvesterRequest, Verkstedregisteret)
        };
        await Task.WhenAll(taskList);
        return UnifyBilpleie(taskList);
    }

    private List<EvidenceValue> UnifyBilpleie(List<Task<List<EvidenceValue>>> taskList)
    {
        string orgnr = string.Empty;
        string godkjenningArbt = string.Empty;
        string registerstatusArbt = string.Empty;
        string godkjenningSVV = string.Empty;
        string godkjenningsnrSVV = string.Empty;
        bool approvedEbevis = false;

        foreach (var task in taskList)
        {
            var evidenceList = task.Result;
            //task has values
            if (evidenceList.Any(x => x.EvidenceValueName == "organisasjonsnummer"))
            {
                //task is arbt or svv
                if (evidenceList.Any(x => x.Source == "Arbeidstilsynet"))
                {
                    godkjenningArbt = evidenceList.First(x => x.EvidenceValueName == "godkjenningsstatus").Value.ToString();
                    orgnr = evidenceList.First(x => x.EvidenceValueName == "organisasjonsnummer").Value.ToString();
                    registerstatusArbt = evidenceList.First(x => x.EvidenceValueName == "registerstatusTekst").Value.ToString();
                }
                else if (evidenceList.Any(x => x.Source == "Statens vegvesen"))
                {
                    orgnr = evidenceList.First(x => x.EvidenceValueName == "organisasjonsnummer").Value.ToString();
                    godkjenningSVV = evidenceList.First(x => x.EvidenceValueName == "godkjenningstyper").Value.ToString();
                    godkjenningsnrSVV = evidenceList.First(x => x.EvidenceValueName == "godkjenningsnumre").Value.ToString();
                }
            }
        }

        approvedEbevis = godkjenningArbt.ToLower().Equals("godkjent") || godkjenningSVV.Length > 5;

        var ecb = new EvidenceBuilder(_evidenceSourceMetadata, BilpleieregisteretEbevisDataset);
        ecb.AddEvidenceValue("organisasjonsnummer", orgnr, EBEVIS, false);
        ecb.AddEvidenceValue("godkjenningsstatusArbeidstilsynet", godkjenningArbt, "");
        ecb.AddEvidenceValue("registerstatusArbeidstilsynet", registerstatusArbt, "");
        ecb.AddEvidenceValue("godkjenningsstatusStatensVegvesen", godkjenningSVV, "");
        ecb.AddEvidenceValue("godkjenningsnumreStatensVegvesen", godkjenningsnrSVV, "");
        ecb.AddEvidenceValue("godkjentEbevis", approvedEbevis, EBEVIS);
        return ecb.GetEvidenceValues();
    }

    /// <summary>
    /// Looks up the subject organisation in the CCR (Enhetsregisteret) through the Dan.Core proxy. Main unit is tried first, then subunit.
    /// </summary>
    private async Task<EntityRegistryUnit> GetCcrUnit(EvidenceHarvesterRequest evidenceHarvesterRequest)
    {
        var organizationNumber = GetSubjectOrganizationNumber(evidenceHarvesterRequest);
        var unit = await _ccrClientService.GetMainUnit(organizationNumber, _settings.DanEnvironment);        
        if (unit == null)
        {
            throw new EvidenceSourcePermanentClientException(ERROR_NOT_FOUND, $"Organisation {organizationNumber} was not found in the central coordinating register");
        }

        return unit;
    }

    private static string GetSubjectOrganizationNumber(EvidenceHarvesterRequest evidenceHarvesterRequest)
    {
        return evidenceHarvesterRequest.SubjectParty?.NorwegianOrganizationNumber ?? evidenceHarvesterRequest.OrganizationNumber;
    }

    private Task<List<EvidenceValue>> GetDataFromBr(EvidenceHarvesterRequest evidenceHarvesterRequest, string evidenceCodeName)
    {
        var request = CloneRequest(evidenceHarvesterRequest, evidenceCodeName);
        return GetDataFromPlugin(request, SourceBr);
    }

    private Task<List<EvidenceValue>> GetDataFromSvv(EvidenceHarvesterRequest evidenceHarvesterRequest, string evidenceCodeName)
    {
        var request = CloneRequest(evidenceHarvesterRequest, evidenceCodeName);
        return GetDataFromPlugin(request, SourceSvv, ignoreErrors: true);
    }

    private async Task<List<EvidenceValue>> GetDataFromArbt(EvidenceHarvesterRequest evidenceHarvesterRequest, string evidenceCodeName)
    {
        var request = CloneRequest(evidenceHarvesterRequest, evidenceCodeName);

        //ARBT only uses main units, so we switch the subject
        var organizationNumber = GetSubjectOrganizationNumber(request);
        var mainUnit = await _ccrClientService.GetMainUnit(organizationNumber, _settings.DanEnvironment);
        var mainUnitOrganizationNumber = mainUnit?.Organisasjonsnummer ?? organizationNumber;

        request.OrganizationNumber = mainUnitOrganizationNumber;
        request.SubjectParty = new Party
        {
            NorwegianOrganizationNumber = mainUnitOrganizationNumber
        };

        return await GetDataFromPlugin(request, SourceArbt, ignoreErrors: true);
    }

    /// <summary>
    /// Calls another DAN plugin through the Dan.Common plugin client, which resolves the plugin URL from source and
    /// environment and attaches a bearer token (PluginScopes / DefaultAzureCredential) in addition to the function key.
    /// </summary>
    /// <param name="ignoreErrors">For bilpleie one of the plugins will return an exception based response, so we need to catch and ignore</param>
    private async Task<List<EvidenceValue>> GetDataFromPlugin(EvidenceHarvesterRequest request, string source, bool ignoreErrors = false)
    {
        try
        {
            var result = await _danPluginClientService.GetPluginDataSetAsync<List<EvidenceValue>>(
                request,
                code: _settings.PluginCode,
                env: _settings.DanEnvironment,
                isDefaultJson: false,
                source: source);

            return result ?? new List<EvidenceValue>();
        }
        catch (Exception e) when (ignoreErrors)
        {
            _logger.LogWarning(e, "Ignoring failed call to {source} plugin for {evidenceCodeName}: {message}",
                source, request.EvidenceCodeName, e.Message);
            return new List<EvidenceValue>();
        }
    }

    /// <summary>
    /// Creates an independent copy of the request with the given dataset name. Several datasets are fetched in parallel
    /// from the same incoming request, so each call needs its own instance.
    /// </summary>
    private static EvidenceHarvesterRequest CloneRequest(EvidenceHarvesterRequest evidenceHarvesterRequest, string evidenceCodeName)
    {
        var clone = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(JsonConvert.SerializeObject(evidenceHarvesterRequest));
        clone.EvidenceCodeName = evidenceCodeName;
        return clone;
    }
}
