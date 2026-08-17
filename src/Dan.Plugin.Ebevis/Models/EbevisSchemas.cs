using System;
using Newtonsoft.Json;

namespace Dan.Plugin.Ebevis.Models
{
    // Model classes used purely to generate JSON schema documentation (via EvidenceValue.SchemaFromObject)
    // for evidence codes in Metadata.cs. These do not change, and are not used by, the actual harvesting
    // logic in Plugin.cs, which continues to return the same individually named EvidenceValues as before.

    // CRITERION.SELECTION.SUITABILITY.TRADE_REGISTER_ENROLMENT
    public class TradeRegisterEnrolment
    {
        public bool IsInRegistryOfBusinessEnterprises { get; set; }
    }

    // CRITERION.SELECTION.ECONOMIC_FINANCIAL_STANDING.TURNOVER.SET_UP
    public class EconomicFinancialStandingTurnoverSetUp
    {
        public DateTime CreatedInCentralRegisterForLegalEntities { get; set; }
        public DateTime Established { get; set; }
    }

    // CRITERION.SELECTION.TECHNICAL_PROFESSIONAL_ABILITY.MANAGEMENT.MANAGERIAL_STAFF (deprecated)
    // and CRITERION.SELECTION.TECHNICAL_PROFESSIONAL_ABILITY.MANAGEMENT.AVERAGE_ANNUAL_MANPOWER
    // share the exact same shape.
    public class ManagerialStaff
    {
        public long NumberOfEmployees { get; set; }
        public Uri CertificateOfRegistrationPdfUrl { get; set; }
    }

    // CRITERION.EXCLUSION.BUSINESS.INSOLVENCY
    public class BusinessInsolvency
    {
        public bool IsBeingDissolved { get; set; }
        public bool IsBeingForciblyDissolved { get; set; }
    }

    // CRITERION.EXCLUSION.BUSINESS.BANKRUPTCY
    public class BusinessBankruptcy
    {
        public bool IsUnderBankruptcy { get; set; }
    }

    // CRITERION.SELECTION.ECONOMIC_FINANCIAL_STANDING.TURNOVER.GENERAL_YEARLY
    public class EconomicFinancialStandingTurnoverGeneralYearly
    {
        public string Year1 { get; set; }
        public Uri Year1PdfUrl { get; set; }
        public string Year2 { get; set; }
        public Uri Year2PdfUrl { get; set; }
        public string Year3 { get; set; }
        public Uri Year3PdfUrl { get; set; }
        public string Year4 { get; set; }
        public Uri Year4PdfUrl { get; set; }
        public string Year5 { get; set; }
        public Uri Year5PdfUrl { get; set; }
    }

    // BilpleieregisteretEbevis. Note: this differs from the existing (unused/stale) Models/BilpleieEbevis.cs,
    // whose fields do not match what Plugin.cs's UnifyBilpleie actually returns as EvidenceValues.
    public class BilpleieregisteretEbevisResult
    {
        [JsonProperty("organisasjonsnummer")]
        public string Organisasjonsnummer { get; set; }

        [JsonProperty("godkjenningsstatusArbeidstilsynet")]
        public string GodkjenningsstatusArbeidstilsynet { get; set; }

        [JsonProperty("registerstatusArbeidstilsynet")]
        public string RegisterstatusArbeidstilsynet { get; set; }

        [JsonProperty("godkjenningsstatusStatensVegvesen")]
        public string GodkjenningsstatusStatensVegvesen { get; set; }

        [JsonProperty("godkjenningsnumreStatensVegvesen")]
        public string GodkjenningsnumreStatensVegvesen { get; set; }

        [JsonProperty("godkjentEbevis")]
        public bool GodkjentEbevis { get; set; }
    }
}
