using Azure.Core.Serialization;
using Dan.Common;
using Dan.Common.Enums;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Dan.Plugin.Ebevis
{

    public class Metadata : IEvidenceSourceMetadata
    {
        private const string SourceCentralUnitRegistry = "BRREG Enhetsregisteret";
        private const string ARBT = "Arbeidstilsynet";
        private const string SVV = "Statens vegvesen";
        private const string EBEVIS = "eBevis";
        private List<string> belongsToEbevis = new List<string>() { "eBevis" };

        [Function(Constants.EvidenceSourceMetadataFunctionName)]
        public async Task<HttpResponseData> GetMetadata([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, FunctionContext context)
        {
            var logger = context.GetLogger(context.FunctionDefinition.Name);
            var response = req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(GetEvidenceCodes(),
                new NewtonsoftJsonObjectSerializer(new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto }));
            return response;
        }

        public List<EvidenceCode> GetEvidenceCodes()
        {
            return new List<EvidenceCode>
            {
                new EvidenceCode()
                {
                    EvidenceCodeName = "CRITERION.SELECTION.SUITABILITY.TRADE_REGISTER_ENROLMENT",
                    Description = "",
                    BelongsToServiceContexts = belongsToEbevis,
                    Values = new List<EvidenceValue>
                    {
                        new EvidenceValue()
                        {
                            EvidenceValueName = "IsInRegistryOfBusinessEnterprises",
                            ValueType = EvidenceValueType.Boolean,
                            Source = SourceCentralUnitRegistry
                        }
                    },
                    AuthorizationRequirements = new List<Requirement>()
                    {
                        new PartyTypeRequirement()
                        {
                            AllowedPartyTypes = new AllowedPartyTypesList()
                            {
                                new KeyValuePair<AccreditationPartyTypes, PartyTypeConstraint>(AccreditationPartyTypes.Requestor,
                                    PartyTypeConstraint.PublicAgency)
                            }
                        }
                    }
                },
                new EvidenceCode()
                {
                    EvidenceCodeName = "CRITERION.SELECTION.ECONOMIC_FINANCIAL_STANDING.TURNOVER.SET_UP",
                    Description = "",
                    BelongsToServiceContexts = belongsToEbevis,
                    Values = new List<EvidenceValue>
                    {
                        new EvidenceValue()
                        {
                            EvidenceValueName = "CreatedInCentralRegisterForLegalEntities",
                            ValueType = EvidenceValueType.DateTime,
                            Source = SourceCentralUnitRegistry
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "Established",
                            ValueType = EvidenceValueType.DateTime,
                            Source = SourceCentralUnitRegistry
                        },
                    },
                    AuthorizationRequirements = new List<Requirement>()
                    {
                        new PartyTypeRequirement()
                        {
                            AllowedPartyTypes = new AllowedPartyTypesList()
                            {
                                new KeyValuePair<AccreditationPartyTypes, PartyTypeConstraint>(AccreditationPartyTypes.Requestor,
                                    PartyTypeConstraint.PublicAgency)
                            }
                        }
                    }
                },
                new EvidenceCode()
                {
                    EvidenceCodeName = "CRITERION.SELECTION.TECHNICAL_PROFESSIONAL_ABILITY.MANAGEMENT.MANAGERIAL_STAFF",
                    Description = "DEPRECATED: Use CRITERION.SELECTION.TECHNICAL_PROFESSIONAL_ABILITY.MANAGEMENT.AVERAGE_ANNUAL_MANPOWER",
                    BelongsToServiceContexts = belongsToEbevis,
                    Values = new List<EvidenceValue>
                    {
                        new EvidenceValue()
                        {
                            EvidenceValueName = "NumberOfEmployees",
                            ValueType = EvidenceValueType.Number,
                            Source = SourceCentralUnitRegistry
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "CertificateOfRegistrationPdfUrl",
                            ValueType = EvidenceValueType.Uri,
                            Source = SourceCentralUnitRegistry
                        },
                    },
                    AuthorizationRequirements = new List<Requirement>()
                    {
                        new PartyTypeRequirement()
                        {
                            AllowedPartyTypes = new AllowedPartyTypesList()
                            {
                                new KeyValuePair<AccreditationPartyTypes, PartyTypeConstraint>(AccreditationPartyTypes.Requestor,
                                    PartyTypeConstraint.PublicAgency)
                            }
                        }
                    }
                },
                new EvidenceCode()
                {
                    EvidenceCodeName = "CRITERION.SELECTION.TECHNICAL_PROFESSIONAL_ABILITY.MANAGEMENT.AVERAGE_ANNUAL_MANPOWER",
                    Description = "",
                    BelongsToServiceContexts = belongsToEbevis,
                    Values = new List<EvidenceValue>
                    {
                        new EvidenceValue()
                        {
                            EvidenceValueName = "NumberOfEmployees",
                            ValueType = EvidenceValueType.Number,
                            Source = SourceCentralUnitRegistry
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "CertificateOfRegistrationPdfUrl",
                            ValueType = EvidenceValueType.Uri,
                            Source = SourceCentralUnitRegistry
                        },
                    },
                    AuthorizationRequirements = new List<Requirement>()
                    {
                        new PartyTypeRequirement()
                        {
                            AllowedPartyTypes = new AllowedPartyTypesList()
                            {
                                new KeyValuePair<AccreditationPartyTypes, PartyTypeConstraint>(AccreditationPartyTypes.Requestor,
                                    PartyTypeConstraint.PublicAgency)
                            }
                        }
                    }
                },
                new EvidenceCode()
                {
                    EvidenceCodeName = "CRITERION.EXCLUSION.BUSINESS.INSOLVENCY",
                    Description = "",
                    BelongsToServiceContexts = belongsToEbevis,
                    Values = new List<EvidenceValue>
                    {
                        new EvidenceValue()
                        {
                            EvidenceValueName = "IsBeingDissolved",
                            ValueType = EvidenceValueType.Boolean,
                            Source = SourceCentralUnitRegistry
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "IsBeingForciblyDissolved",
                            ValueType = EvidenceValueType.Boolean,
                            Source = SourceCentralUnitRegistry
                        },
                    },
                    AuthorizationRequirements = new List<Requirement>()
                    {
                        new PartyTypeRequirement()
                        {
                            AllowedPartyTypes = new AllowedPartyTypesList()
                            {
                                new KeyValuePair<AccreditationPartyTypes, PartyTypeConstraint>(AccreditationPartyTypes.Requestor,
                                    PartyTypeConstraint.PublicAgency)
                            }
                        }
                    }
                },
                new EvidenceCode()
                {
                    EvidenceCodeName = "CRITERION.EXCLUSION.BUSINESS.BANKRUPTCY",
                    Description = "",
                    BelongsToServiceContexts = belongsToEbevis,
                    Values = new List<EvidenceValue>
                    {
                        new EvidenceValue()
                        {
                            EvidenceValueName = "IsUnderBankruptcy",
                            ValueType = EvidenceValueType.Boolean,
                            Source = SourceCentralUnitRegistry
                        },
                    },
                    AuthorizationRequirements = new List<Requirement>()
                    {
                        new PartyTypeRequirement()
                        {
                            AllowedPartyTypes = new AllowedPartyTypesList()
                            {
                                new KeyValuePair<AccreditationPartyTypes, PartyTypeConstraint>(AccreditationPartyTypes.Requestor,
                                    PartyTypeConstraint.PublicAgency)
                            }
                        }
                    }
                },
                new EvidenceCode()
                {
                    EvidenceCodeName = "CRITERION.SELECTION.ECONOMIC_FINANCIAL_STANDING.TURNOVER.GENERAL_YEARLY",
                    Description = "Code for retrieving URLs to PDFs for annual financial reports (1-5 years)",
                    BelongsToServiceContexts = belongsToEbevis,
                    Parameters = new List<EvidenceParameter>()
                    {
                        new EvidenceParameter()
                        {
                            EvidenceParamName = "NumberOfYears",
                            ParamType = EvidenceParamType.Number,
                            Required = true
                        }
                    },
                    Values = new List<EvidenceValue>
                    {
                        new EvidenceValue()
                        {
                            EvidenceValueName = "Year1",
                            ValueType = EvidenceValueType.String,
                            Source = SourceCentralUnitRegistry
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "Year1PdfUrl",
                            ValueType = EvidenceValueType.Uri,
                            Source = SourceCentralUnitRegistry
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "Year2",
                            ValueType = EvidenceValueType.String,
                            Source = SourceCentralUnitRegistry
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "Year2PdfUrl",
                            ValueType = EvidenceValueType.Uri,
                            Source = SourceCentralUnitRegistry
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "Year3",
                            ValueType = EvidenceValueType.String,
                            Source = SourceCentralUnitRegistry
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "Year3PdfUrl",
                            ValueType = EvidenceValueType.Uri,
                            Source = SourceCentralUnitRegistry
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "Year4",
                            ValueType = EvidenceValueType.String,
                            Source = SourceCentralUnitRegistry
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "Year4PdfUrl",
                            ValueType = EvidenceValueType.Uri,
                            Source = SourceCentralUnitRegistry
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "Year5",
                            ValueType = EvidenceValueType.String,
                            Source = SourceCentralUnitRegistry
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "Year5PdfUrl",
                            ValueType = EvidenceValueType.Uri,
                            Source = SourceCentralUnitRegistry
                        },
                    },
                    AuthorizationRequirements = new List<Requirement>()
                    {
                        new PartyTypeRequirement()
                        {
                            AllowedPartyTypes = new AllowedPartyTypesList()
                            {
                                new KeyValuePair<AccreditationPartyTypes, PartyTypeConstraint>(AccreditationPartyTypes.Requestor,
                                    PartyTypeConstraint.PublicAgency)
                            }
                        }
                    }
                },
                new EvidenceCode()
                {
                    EvidenceCodeName = "BilpleieregisteretEbevis",
                    Description = "Kombinerer bilpleieregisteret til Arbeidstilsynet og verkstedregisteret til Statens vegvesen",
                    BelongsToServiceContexts = belongsToEbevis,
                    Values = new List<EvidenceValue>
                    {
                        new ()
                        {
                            EvidenceValueName = "organisasjonsnummer",
                            ValueType = EvidenceValueType.String,
                            Source = EBEVIS
                        },
                        new()
                        {
                            EvidenceValueName = "godkjenningsstatusArbeidstilsynet",
                            ValueType = EvidenceValueType.String,
                            Source = ARBT,
                        },
                        new()
                        {
                            EvidenceValueName = "registerstatusArbeidstilsynet",
                            ValueType = EvidenceValueType.String,
                            Source = ARBT,
                        },
                        new()
                        {
                            EvidenceValueName = "godkjenningsstatusStatensVegvesen",
                            ValueType = EvidenceValueType.String,
                            Source = SVV,
                        },
                        new()
                        {
                            EvidenceValueName = "godkjenningsnumreStatensVegvesen",
                            ValueType = EvidenceValueType.String,
                            Source = SVV,
                        },
                        new()
                        {
                            EvidenceValueName = "godkjentEbevis",
                            ValueType = EvidenceValueType.Boolean,
                            Source = EBEVIS
                        }
                    },
                    AuthorizationRequirements = new List<Requirement>()
                    {
                        new PartyTypeRequirement()
                        {
                            AllowedPartyTypes = new AllowedPartyTypesList()
                            {
                                new KeyValuePair<AccreditationPartyTypes, PartyTypeConstraint>(AccreditationPartyTypes.Requestor,
                                    PartyTypeConstraint.PublicAgency)
                            }
                        }
                    }
                }
            };
        }
    }
}


