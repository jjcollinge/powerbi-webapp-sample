using KPMGSample.Models;
using Microsoft.AspNet.Identity;
using Microsoft.PowerBI.Api.V1;
using Microsoft.PowerBI.Security;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

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
            /**
             * AppKey: Authorization and comes from Azure PowerBi embedded workspace
             * AppToken: JWT signed by AppKey to embed HTTP requests [username + role for row level security]
             * ApiUrl: Azure PowerBi HTTP endpoint
             * Workspace collection: Name of the Azure workspace
             * WorkspaceId: A workspace id within workspace collection
             **/

            this.workspaceCollection = ConfigurationManager.AppSettings["powerbi:WorkspaceCollection"];
            this.workspaceId = ConfigurationManager.AppSettings["powerbi:WorkspaceId"];
            this.accessKey = ConfigurationManager.AppSettings["powerbi:AccessKey"];
            this.apiUrl = ConfigurationManager.AppSettings["powerbi:ApiUrl"];
            this.appKey = "AppKey";
        }

        [Authorize]
        public ActionResult Index()
        {
            // List PowerBi reports

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
                var username = User.Identity.GetUserName();
                var roles = Roles.GetRolesForUser();

                ViewBag["username"] = username;
                ViewBag["roles"] = roles;

                // Silently swallow exception
                return PartialView();
            }   
        }

        public async Task<ActionResult> Report(string reportId)
        {
            // Display specific PowerBi report

            using (var client = this.CreatePowerBIClient())
            {
                var reportsResponse = await client.Reports.GetReportsAsync(this.workspaceCollection, this.workspaceId);
                var report = reportsResponse.Value.FirstOrDefault(r => r.Id == reportId);

                // Grab username and role
                var username = User.Identity.GetUserName();
                var roles = Roles.GetRolesForUser(username);

                var embedToken = PowerBIToken.CreateReportEmbedToken(this.workspaceCollection, this.workspaceId, report.Id, username, roles);

                var viewModel = new ReportViewModel
                {
                    Report = report,
                    AccessToken = embedToken.Generate(this.accessKey)
                };

                return View(viewModel); // Need to create view
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