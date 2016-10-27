using KPMGSample.Models;
using Microsoft.PowerBI.Api.V1;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KPMGSample.Controllers
{
    public class ReportsController : Controller
    {
        private readonly string workspaceCollection;
        private readonly string workspaceId;
        private readonly string accessKey;
        private readonly string apiUrl;
        private readonly string appKey;

        public ReportsController()
        {
            this.workspaceCollection = ConfigurationManager.AppSettings["powerbi:WorkspaceCollection"];
            this.workspaceId = ConfigurationManager.AppSettings["powerbi:WorkspaceId"];
            this.accessKey = ConfigurationManager.AppSettings["powerbi:AccessKey"];
            this.apiUrl = ConfigurationManager.AppSettings["powerbi:ApiUrl"];
            this.appKey = "AppKey";
        }

        [Authorize]
        public ActionResult Index()
        {
            try
            {
                using (var client = this.CreatePowerBIClient())
                {
                    var reportsResponse = client.Reports.GetReports(this.workspaceCollection, this.workspaceId);

                    var viewModel = new ReportsViewModel
                    {
                        Reports = reportsResponse.Value.ToList()
                    };

                    return PartialView(viewModel);
                }
            }
            catch (Exception ex)
            {
                // Silently swallow exception
                return PartialView();
            }   
        }
        private IPowerBIClient CreatePowerBIClient()
        {
            var credentials = new TokenCredentials(accessKey, appKey);
            var client = new PowerBIClient(credentials)
            {
                BaseUri = new Uri(apiUrl)
            };

            return client;
        }
    }
}