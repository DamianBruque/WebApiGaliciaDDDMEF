using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using System.Diagnostics;
using System.Xml.Linq;
using InMotionGIT.Underwriting.Contracts.Enumerations;
using InMotionGIT.Argentina.Entity.Contracts;
using InMotionGIT.Policy.Entity.Contracts;
using InMotionGIT.Client.Entity.Contracts;
using InMotionGIT.Product.Entity.Contracts;
using InMotionGIT.CommercialStructure.Proxy.client;
using InMotionGIT.Underwriting.Proxy;
using Domain.Contracts.Interface.ValidationRoutines;
using Domain.Entities;
using System.ComponentModel.Composition;
using System.Data;

public class ProductAccumulationAll
{
    public DbConnection currentConnection { get; set; }

    public bool Calculate(InMotionGIT.Underwriting.Contracts.UnderwritingCase underwritingCase, ref EnumRequirementStatus requirementStatus, ref string commentary, ref Common.Contracts.Errors.ErrorCollection errors)
    {
        bool requirement = false;

        #region Atributos
        var SummaryOfPoliciesByClientClassCollection = new InMotionGIT.Argentina.Entity.Contracts.SummaryOfPoliciesByClientClassCollection();
        InMotionGIT.Underwriting.Contracts.UnderwritingCase UnderWritingCasewithClient = null;
        var UnderwritingManager = new InMotionGIT.Underwriting.Proxy.Manager();
        List<InMotionGIT.Underwriting.Contracts.UnderwritingCase> UnderWritingCaseList;
        bool groupIndicator = false;
        #endregion

        if (underwritingCase.RiskInformation.Modules.IsNotEmpty() && underwritingCase.RiskInformation.Modules.Count > 0)
        {
            foreach (var moduleItem in underwritingCase.RiskInformation.Modules)
            {
                ProcessModules(underwritingCase, ref groupIndicator, ref SummaryOfPoliciesByClientClassCollection, ref UnderWritingCaseList, ref UnderwritingManager, ref UnderWritingCasewithClient, ref requirementStatus, ref commentary, ref errors, ref requirement,ref moduleItem)
            }
        }
        return requirement;
    }

    private void ProcessModules(InMotionGIT.Underwriting.Contracts.UnderwritingCase underwritingCase, ref bool groupIndicator, ref InMotionGIT.Argentina.Entity.Contracts.SummaryOfPoliciesByClientClassCollection SummaryOfPoliciesByClientClassCollection, ref List<InMotionGIT.Underwriting.Contracts.UnderwritingCase> UnderWritingCaseList, ref InMotionGIT.Underwriting.Proxy.Manager UnderwritingManager, ref InMotionGIT.Underwriting.Contracts.UnderwritingCase UnderWritingCasewithClient, ref EnumRequirementStatus requirementStatus, ref string commentary, ref Common.Contracts.Errors.ErrorCollection errors, ref bool requirement, ref Policy.Entity.Contracts moduleItem)
    {
        #region Atributos
        bool blnValid = true;
        InMotionGIT.Policy.Entity.Contracts.RoleCollection roles = null;
        Policy.Entity.Contracts.RiskInformation risk = null;
        Common.Contracts.Process.ConsumerInformation consumer = null;
        InMotionGIT.Policy.Entity.Contracts.AutomaticPaymentPolicyCollection AutomaticPaymetPolicy = null;
        InMotionGIT.Underwriting.Contracts.UnderwritingCase UnderWritingCasewithoutclient = null;
        InMotionGIT.Argentina.Entity.Contracts.SummaryOfPoliciesByClientClass SummaryOfPoliciesByClientClass;
        bool Exists = false;
        bool requested = false;
        string productAcumulate = null;
        int _count_pol = 0;
        var ArgentinaClient = new InMotionGIT.Argentina.Services.Client.Client();
        string DocumentNumberAux = null;
        InMotionGIT.Client.Entity.Contracts.ClientDocument CliDocument;
        InMotionGIT.Client.Entity.Contracts.ClientDocumentCollection CliDocumentCollection;
        bool clientFind = false;
        string result;
        int countResultVT1 = 0;
        int countResultVT7 = 0;
        string detailsResultVT1 = "";
        bool resultVT7 = false;
        long policyID = 0;
        InMotionGIT.Product.Entity.Contracts.GroupProductDetailCollection groupProductDetailCollection;
        InMotionGIT.CommercialStructure.Proxy.client.StrucProductCollection strucProductsCollection; //---
        var commercialStructureProxy = new InMotionGIT.CommercialStructure.Proxy.client.StructProductsManagerClient(); //---
        long certificateID = 0;
        #endregion
        foreach (var module in underwritingCase.RiskInformation.Modules)
        {
            strucProductsCollection = commercialStructureProxy.RetrieveStrucProductByLineOfBusinessProductModule(underwritingCase.RiskInformation.CommercialStructureAppliedByEvent.CommercialStructureID, underwritingCase.RiskInformation.LineOfBusiness, underwritingCase.RiskInformation.ProductCode, underwritingCase.RiskInformation.CommercialStructureAppliedByEvent.EffectiveDate, moduleItem.CoverageModule);
            if (strucProductsCollection.IsNotEmpty() && strucProductsCollection.Count > 0)
            {
                foreach (var strucProductItem in strucProductsCollection)
                {
                    if (strucProductItem.GroupCode.IsNotEmpty() && strucProductItem.GroupCode > 0)
                    {
                        groupProductDetailCollection = InMotionGIT.Product.Entity.Contracts.GroupProductDetail.Retrieve(strucProductItem.GroupCode, DateTime.Today, string.Empty);
                        if (groupProductDetailCollection.IsNotEmpty() && groupProductDetailCollection.Count > 0)
                        {
                            groupIndicator = true;
                        }
                    }
                    CliDocument = (from r in underwritingCase.RiskInformation.PrimaryInsured.Client.ClientDocuments where r.DocumentType == 5 || r.DocumentType == 1 select r).FirstOrDefault();   //HAB-4517
                    CliDocument.DocumentNUmber = ArgentinaClient.FormatDocumentNumber(CliDocument.DocumentNUmber, CliDocument.DocumentType);
                    CliDocumentCollection = InMotionGIT.Client.Entity.Contracts.ClientDocument.RetrieveByCondition(string.Format("WHERE NTYPCLIENTDOC = {0} AND SCLINUMDOCU  = '{1}'", CliDocument.DocumentType, CliDocument.DocumentNUmber), DateTime.Today, InMotionGIT.Client.Entity.Contracts.ClientDocument.FilterSetting(InMotionGIT.Client.Entity.Contracts.Enumerations.EnumClientDocumentChild.None));
                    if (!groupIndicator)
                    {
                        ProcessNonGroupIndicator(underwritingCase, CliDocumentCollection, ref clientFind, ref policyID, ref certificateID, ref SummaryOfPoliciesByClientClassCollection, ref UnderWritingCaseList, ref UnderwritingManager, ref UnderWritingCasewithClient);
                    }
                    else
                    {
                        ProcessGroupIndicator(underwritingCase, CliDocumentCollection, groupProductDetailCollection, ref clientFind, ref policyID, ref certificateID, ref SummaryOfPoliciesByClientClassCollection, ref UnderWritingCaseList, ref UnderwritingManager, ref UnderWritingCasewithClient);
                    }

                    try
                    {
                        using (var ServiceVT1 = new ServiceReference.ServiceSoapClient())
                        {
                            result = ServiceVT1.LeerCumulosPorProducto(CliDocument.DocumentType, CliDocument.DocumentNUmber, underwritingCase.RiskInformation.PrimaryInsured.Client.Gender, underwritingCase.RiskInformation.EffectiveDate.ToString("ddMMyyyy"), underwritingCase.RiskInformation.LineOfBusiness, underwritingCase.RiskInformation.ProductCode);
                        }

                        if (!string.IsNullOrEmpty(result))
                        {
                            var xmlDocument = XDocument.Parse(result);
                            if (xmlDocument.Descendants("xml").Elements("cantidadRegistros").Value != "")
                            {
                                countResultVT1 = Convert.ToInt32(xmlDocument.Descendants("xml").Elements("cantidadRegistros").Value);
                                detailsResultVT1 = xmlDocument.Descendants("xml").Elements("detalleRegistros").Value;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    if (SummaryOfPoliciesByClientClassCollection.IsNotEmpty())
                    {
                        countResultVT7 = SummaryOfPoliciesByClientClassCollection.Count;
                    }
                    if (countResultVT7 + countResultVT1 >= strucProductItem.MaximunAccumulation.IfEmpty(1))
                    {
                        if (countResultVT7 > 0)
                        {
                            foreach (var lstCoverByPolicyItem in SummaryOfPoliciesByClientClassCollection)
                            {
                                if (lstCoverByPolicyItem.Status == 4 || lstCoverByPolicyItem.Status == 5)
                                {
                                    requested = true;
                                    resultVT7 = true;
                                    productAcumulate += (productAcumulate == "" ? "" : " /") + (lstCoverByPolicyItem.PolicyID > 0 ? "Cert/Pol: " + lstCoverByPolicyItem.LineOfBusiness.ToString() + "/" + lstCoverByPolicyItem.ProductCode.ToString() + "/" + lstCoverByPolicyItem.PolicyID.ToString() + "/" + lstCoverByPolicyItem.CertificateID.ToString() : "Sol: " + lstCoverByPolicyItem.CaseNumber.ToString()) +
                                                        (lstCoverByPolicyItem.PolicyID > 0 ? " Vigencia " + lstCoverByPolicyItem.RecordEffectiveDate.ToString("dd/MM/yyyy") + "-" + lstCoverByPolicyItem.EndingDate.ToString("dd/MM/yyyy") : " Creación " + lstCoverByPolicyItem.RecordEffectiveDate.ToString("dd/MM/yyyy"));
                                }
                            }
                        }
                        if (countResultVT1 > 0)
                        {
                            requested = true;
                        }
                        if (requested)
                        {
                            requirementStatus = EnumRequirementStatus.Rejected;
                            commentary = "Requerimiento rechazado por cúmulo. " + (resultVT7 ? " VT7: " + productAcumulate : "") + (countResultVT1 > 0 ? " VT1: " + detailsResultVT1 : "");
                            requirement = true;
                        }
                        else
                        {
                            requirementStatus = EnumRequirementStatus.None;
                            commentary = "";
                            requirement = false;
                        }
                    }
                }
            }
        }
    }

    private void ProcessNonGroupIndicator(InMotionGIT.Underwriting.Contracts.UnderwritingCase underwritingCase, InMotionGIT.Client.Entity.Contracts.ClientDocumentCollection CliDocumentCollection, ref bool clientFind, ref long policyID, ref long certificateID, ref InMotionGIT.Argentina.Entity.Contracts.SummaryOfPoliciesByClientClassCollection SummaryOfPoliciesByClientClassCollection, ref List<InMotionGIT.Underwriting.Contracts.UnderwritingCase> UnderWritingCaseList, ref InMotionGIT.Underwriting.Proxy.Manager UnderwritingManager, ref InMotionGIT.Underwriting.Contracts.UnderwritingCase UnderWritingCasewithClient)
    {
        if (CliDocumentCollection.Count > 0)
        {
            foreach (var clienDocumentItem in CliDocumentCollection)
            {
                clientFind = InMotionGIT.Client.Entity.Contracts.Client.FindByCondition($"WHERE SCLIENT = '{clienDocumentItem.ClientID}' AND SSEXCLIEN  = '{underwritingCase.RiskInformation.PrimaryInsured.Client.Gender}'");

                if (clientFind)
                {
                    var roles = InMotionGIT.Policy.Entity.Contracts.Role.RetrieveByCondition($"WHERE SCLIENT = '{underwritingCase.RiskInformation.PrimaryInsured.ClientID}' AND NROLE  IN (1,2) AND NBRANCH = {underwritingCase.RiskInformation.LineOfBusiness} AND NPRODUCT = {underwritingCase.RiskInformation.ProductCode}", DateTime.Today, InMotionGIT.Policy.Entity.Contracts.Role.FilterSetting(Policy.Entity.Contracts.Enumerations.EnumRoleChild.None));

                    foreach (var rolesItem in roles.OrderBy(r => r.PolicyID))
                    {
                        if ((rolesItem.PolicyID != policyID || (rolesItem.CertificateID > 0 && rolesItem.CertificateID != certificateID) || rolesItem.CertificateID == 0))
                        {
                            var risk = (new InMotionGIT.PolicyManager.Services.RiskManager()).Retrieve(recordType: rolesItem.RecordType,
                                                                                                        lineOfBusiness: rolesItem.LineOfBusiness,
                                                                                                        productCode: rolesItem.ProductCode,
                                                                                                        policyID: rolesItem.PolicyID,
                                                                                                        certificateID: rolesItem.CertificateID,
                                                                                                        atDate: DateTime.Today,
                                                                                                        includeClientInformation: true,
                                                                                                        consumer: consumer);
                            if (risk != null)
                            {
                                var SummaryOfPoliciesByClientClass = new InMotionGIT.Argentina.Entity.Contracts.SummaryOfPoliciesByClientClass
                                {
                                    RecordType = risk.RecordType,
                                    LineOfBusiness = risk.LineOfBusiness,
                                    ProductCode = risk.ProductCode,
                                    PolicyID = risk.PolicyID,
                                    CertificateID = risk.CertificateID,
                                    RecordEffectiveDate = risk.EffectiveDate,
                                    EndingDate = risk.EndingDate,
                                    Status = risk.StatusOfPolicyCertificate,
                                    CancelationCode = risk.CancellationCode,
                                    PayForm = risk.PaymentMethod
                                };
                                if (risk.AutomaticPaymentPolicy != null)
                                {
                                    SummaryOfPoliciesByClientClass.BankAccount = risk.AutomaticPaymentPolicy.BankAccount;
                                    SummaryOfPoliciesByClientClass.CreditCardType = risk.AutomaticPaymentPolicy.CreditCardType;
                                    SummaryOfPoliciesByClientClass.CreditCardNumber = risk.AutomaticPaymentPolicy.CreditCardNumber;
                                }
                                SummaryOfPoliciesByClientClassCollection.Add(SummaryOfPoliciesByClientClass);
                            }
                        }

                        policyID = rolesItem.PolicyID;
                        certificateID = rolesItem.CertificateID;
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(underwritingCase.RiskInformation.PrimaryInsured.ClientID))
        {
            if (UnderWritingCaseList == null)
            {
                UnderWritingCaseList = UnderwritingManager.RetrieveUnderwritingCaseByClientId(underwritingCase.RiskInformation.PrimaryInsured.ClientID, underwritingCase.RiskInformation.PrimaryInsured.ClientRole, 0, 0);
            }
            if (UnderWritingCaseList.Count > 0)
            {
                foreach (var UnderWritingCaseItem in UnderWritingCaseList.Where(r => r.Decision != 2 && r.Decision != 3 && r.UnderwritingCaseID != underwritingCase.UnderwritingCaseID))
                {
                    try
                    {
                        var UnderWritingCasewithClient = InMotionGIT.Underwriting.Proxy.Manager.Retrieve(UnderWritingCaseItem.UnderwritingCaseID, DateTime.Today, false, true, true, false);

                        if (UnderWritingCasewithClient.RiskInformation.LineOfBusiness == underwritingCase.RiskInformation.LineOfBusiness && UnderWritingCasewithClient.RiskInformation.ProductCode == underwritingCase.RiskInformation.ProductCode)
                        {
                            var SummaryOfPoliciesByClientClass = new InMotionGIT.Argentina.Entity.Contracts.SummaryOfPoliciesByClientClass
                            {
                                CaseNumber = UnderWritingCasewithClient.UnderwritingCaseID,
                                RecordType = UnderWritingCasewithClient.RiskInformation.RecordType,
                                LineOfBusiness = UnderWritingCasewithClient.RiskInformation.LineOfBusiness,
                                ProductCode = UnderWritingCasewithClient.RiskInformation.ProductCode,
                                PolicyID = UnderWritingCasewithClient.RiskInformation.PolicyID,
                                CertificateID = UnderWritingCasewithClient.RiskInformation.CertificateID,
                                RecordEffectiveDate = UnderWritingCasewithClient.CreationDate,
                                Status = 4,
                                CancelationCode = UnderWritingCasewithClient.RiskInformation.CancellationCode,
                                PayForm = UnderWritingCasewithClient.RiskInformation.PaymentMethod
                            };
                            if (UnderWritingCasewithClient.RiskInformation.AutomaticPaymentPolicy != null)
                            {
                                SummaryOfPoliciesByClientClass.BankAccount = UnderWritingCasewithClient.RiskInformation.AutomaticPaymentPolicy.BankAccount;
                                SummaryOfPoliciesByClientClass.CreditCardType = UnderWritingCasewithClient.RiskInformation.AutomaticPaymentPolicy.CreditCardType;
                                SummaryOfPoliciesByClientClass.CreditCardNumber = UnderWritingCasewithClient.RiskInformation.AutomaticPaymentPolicy.CreditCardNumber;
                            }
                            SummaryOfPoliciesByClientClassCollection.Add(SummaryOfPoliciesByClientClass);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
        }
    }

    private void ProcessGroupIndicator(InMotionGIT.Underwriting.Contracts.UnderwritingCase underwritingCase, InMotionGIT.Client.Entity.Contracts.ClientDocumentCollection CliDocumentCollection, InMotionGIT.Product.Entity.Contracts.GroupProductDetailCollection groupProductDetailCollection, ref bool clientFind, ref long policyID, ref long certificateID, ref InMotionGIT.Argentina.Entity.Contracts.SummaryOfPoliciesByClientClassCollection SummaryOfPoliciesByClientClassCollection, ref List<InMotionGIT.Underwriting.Contracts.UnderwritingCase> UnderWritingCaseList, ref InMotionGIT.Underwriting.Proxy.Manager UnderwritingManager, ref InMotionGIT.Underwriting.Contracts.UnderwritingCase UnderWritingCasewithClient)
    {
        foreach (var groupProductDetail in groupProductDetailCollection)
        {
            if (CliDocumentCollection.Count > 0)
            {
                foreach (var clienDocumentItem in CliDocumentCollection)
                {
                    var clientFind = InMotionGIT.Client.Entity.Contracts.Client.FindByCondition($"WHERE SCLIENT = '{clienDocumentItem.ClientID}' AND SSEXCLIEN  = '{underwritingCase.RiskInformation.PrimaryInsured.Client.Gender}'");

                    if (clientFind)
                    {
                        var roles = InMotionGIT.Policy.Entity.Contracts.Role.RetrieveByCondition($"WHERE SCLIENT = '{underwritingCase.RiskInformation.PrimaryInsured.ClientID}' AND NROLE  IN (1,2) AND NBRANCH = {groupProductDetail.LineBusiness} AND NPRODUCT = {groupProductDetail.ProductCode}", DateTime.Today, InMotionGIT.Policy.Entity.Contracts.Role.FilterSetting(Policy.Entity.Contracts.Enumerations.EnumRoleChild.None));

                        foreach (var rolesItem in roles.OrderBy(r => r.PolicyID))
                        {
                            if ((rolesItem.PolicyID != policyID || underwritingCase.RiskInformation.Policy.PolicyType == 2) && ((rolesItem.CertificateID > 0 && rolesItem.CertificateID != certificateID) || rolesItem.CertificateID == 0))
                            {
                                var risk = (new InMotionGIT.PolicyManager.Services.RiskManager()).Retrieve(recordType: rolesItem.RecordType,
                                                                                                          lineOfBusiness: rolesItem.LineOfBusiness,
                                                                                                          productCode: rolesItem.ProductCode,
                                                                                                          policyID: rolesItem.PolicyID,
                                                                                                          certificateID: rolesItem.CertificateID,
                                                                                                          atDate: DateTime.Today,
                                                                                                          includeClientInformation: true,
                                                                                                          consumer: consumer);
                                if (risk != null && (risk.StatusOfPolicyCertificate == 4 || risk.StatusOfPolicyCertificate == 5))
                                {
                                    var SummaryOfPoliciesByClientClass = new InMotionGIT.Argentina.Entity.Contracts.SummaryOfPoliciesByClientClass
                                    {
                                        RecordType = risk.RecordType,
                                        LineOfBusiness = risk.LineOfBusiness,
                                        ProductCode = risk.ProductCode,
                                        PolicyID = risk.PolicyID,
                                        CertificateID = risk.CertificateID,
                                        RecordEffectiveDate = risk.EffectiveDate,
                                        EndingDate = risk.EndingDate,
                                        Status = risk.StatusOfPolicyCertificate,
                                        CancelationCode = risk.CancellationCode,
                                        PayForm = risk.PaymentMethod
                                    };
                                    if (risk.AutomaticPaymentPolicy != null)
                                    {
                                        SummaryOfPoliciesByClientClass.BankAccount = risk.AutomaticPaymentPolicy.BankAccount;
                                        SummaryOfPoliciesByClientClass.CreditCardType = risk.AutomaticPaymentPolicy.CreditCardType;
                                        SummaryOfPoliciesByClientClass.CreditCardNumber = risk.AutomaticPaymentPolicy.CreditCardNumber;
                                    }
                                    SummaryOfPoliciesByClientClassCollection.Add(SummaryOfPoliciesByClientClass);
                                }
                            }

                            policyID = rolesItem.PolicyID;
                            certificateID = rolesItem.CertificateID;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(underwritingCase.RiskInformation.PrimaryInsured.ClientID))
            {
                if (UnderWritingCaseList == null)
                {
                    UnderWritingCaseList = UnderwritingManager.RetrieveUnderwritingCaseByClientId(underwritingCase.RiskInformation.PrimaryInsured.ClientID, underwritingCase.RiskInformation.PrimaryInsured.ClientRole, 0, 0);
                }
                if (UnderWritingCaseList.Count > 0)
                {
                    foreach (var UnderWritingCaseItem in UnderWritingCaseList.Where(r => r.Decision != 2 && r.Decision != 3 && r.UnderwritingCaseID != underwritingCase.UnderwritingCaseID))
                    {
                        try
                        {
                            var UnderWritingCasewithClient = InMotionGIT.Underwriting.Proxy.Manager.Retrieve(UnderWritingCaseItem.UnderwritingCaseID, DateTime.Today, false, true, true, false);

                            if (UnderWritingCasewithClient.RiskInformation.LineOfBusiness == groupProductDetail.LineBusiness && UnderWritingCasewithClient.RiskInformation.ProductCode == groupProductDetail.ProductCode)
                            {
                                var SummaryOfPoliciesByClientClass = new InMotionGIT.Argentina.Entity.Contracts.SummaryOfPoliciesByClientClass
                                {
                                    CaseNumber = UnderWritingCasewithClient.UnderwritingCaseID,
                                    RecordType = UnderWritingCasewithClient.RiskInformation.RecordType,
                                    LineOfBusiness = UnderWritingCasewithClient.RiskInformation.LineOfBusiness,
                                    ProductCode = UnderWritingCasewithClient.RiskInformation.ProductCode,
                                    PolicyID = UnderWritingCasewithClient.RiskInformation.PolicyID,
                                    CertificateID = UnderWritingCasewithClient.RiskInformation.CertificateID,
                                    RecordEffectiveDate = UnderWritingCasewithClient.CreationDate,
                                    Status = 4,
                                    CancelationCode = UnderWritingCasewithClient.RiskInformation.CancellationCode,
                                    PayForm = UnderWritingCasewithClient.RiskInformation.PaymentMethod
                                };
                                if (UnderWritingCasewithClient.RiskInformation.AutomaticPaymentPolicy != null)
                                {
                                    SummaryOfPoliciesByClientClass.BankAccount = UnderWritingCasewithClient.RiskInformation.AutomaticPaymentPolicy.BankAccount;
                                    SummaryOfPoliciesByClientClass.CreditCardType = UnderWritingCasewithClient.RiskInformation.AutomaticPaymentPolicy.CreditCardType;
                                    SummaryOfPoliciesByClientClass.CreditCardNumber = UnderWritingCasewithClient.RiskInformation.AutomaticPaymentPolicy.CreditCardNumber;
                                }
                                SummaryOfPoliciesByClientClassCollection.Add(SummaryOfPoliciesByClientClass);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }
    }

}
