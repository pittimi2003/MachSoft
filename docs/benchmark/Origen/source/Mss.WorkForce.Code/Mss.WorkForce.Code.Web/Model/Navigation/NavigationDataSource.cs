using Mss.WorkForce.Code.Web.Services;

namespace Mss.WorkForce.Code.Web.Model
{
    public class NavigationDataSource
    {
        #region Fields

        private readonly ILocalizationService _l;
		private readonly Dictionary<string, MenuItem> _navigationItems;

        public NavigationDataSource(ILocalizationService localization)
		{
			_l = localization;
            _navigationItems = new Dictionary<string, MenuItem>
			{
                //Menu WORKING PLANNING
				{ "workplanning" , new MenuItem
					{
						Name = _l.Loc("WORKING PLANNING"),
						IsParent = true,
						BreadcrumbChildren = new List<MenuItem>
						{
							new() {Name = _l.Loc("Live schedule"), Url = "/live-schedule", Icon = "mlx-icon-menu mlx-ico-planification"},
                            new() {Name = _l.Loc("Workers operations"), Url = "/workers-operations", Icon = "mlx-icon-menu mlx-ico-workers-light"},
                            new() {Name = _l.Loc("Equipment operations"), Url = "/equipments-operations", Icon = "mlx-icon-menu mlx-ico-equipment-light"},
						}
					}
				},
				{ "live-schedule", new MenuItem
					{
						Name = _l.Loc("Live schedule"),
						DependsOnWarehouse = true,
						Icon = "mlx-icon-menu mlx-ico-planification",
						Url = "/live-schedule"
                    }
				},				
				{ "workers-operations", new MenuItem
					{
						Name = _l.Loc("Workers operations"),
						DependsOnWarehouse = true,
						Icon = "mlx-icon-menu mlx-ico-workers-light",
						Url = "/workers-operations"
                    }
				},
				{ "equipments-operations", new MenuItem
					{
						Name = _l.Loc("Equipment operations"),
						DependsOnWarehouse = true,
						Icon = "mlx-icon-menu mlx-ico-equipment-light",
						Url = "/equipments-operations"
                    }
				},
				//{ "yard-operations", new MenuItem
				//	{
				//		Name = _l.Loc("Yard operations"),
				//		DependsOnWarehouse = true,
				//		Icon = "mlx-icon-menu mlx-ico-yard-light",
				//		Url = "/yard-operations"
				//	}
				//},
				{"scenariosimulationanddesing", new MenuItem
					{
						Name = _l.Loc("SCENARIO SIMULATION AND DESIGN"),
						IsParent = true,
						BreadcrumbChildren = new List<MenuItem>
						{
                            new() {Name = _l.Loc("Scenarios planner"), Url = "/scenario-planner", Icon = "mlx-icon-menu mlx-ico-preview"},
							new() { Name = _l.Loc("Project designer"), Url = "/project-designer", Icon = "mlx-icon-menu mlx-ico-brush-light"},
                        }
                    }

                },
                { "scenario-planner", new MenuItem
                    {
                        Name = _l.Loc("Scenarios planner"),
                        DependsOnWarehouse = true,
                        Icon = "mlx-icon-menu mlx-ico-preview",
                        Url = "/scenario-planner"
                    }
                },
                { "project-designer", new MenuItem
                    {
                        Name = _l.Loc("Project designer"),
                        DependsOnWarehouse = true,
                        Icon = "mlx-icon-menu mlx-ico-brush-light",
                        Url = "/project-designer"
                    }
                },
				//Menu MASTER DATA
				{"masterdata" , new MenuItem{

					Name = _l.Loc("MASTER DATA"),
					IsParent = true,
                    BreadcrumbChildren = new List<MenuItem>
					{
						new() { Name = _l.Loc("Organization"), Url = "/organization", Icon = "mlx-icon-menu mlx-ico-organization-light" },
						new() { Name = _l.Loc("Warehouses"), Url = "/warehouses", Icon = "mlx-icon-menu mlx-ico-warehouse-light" },
						new() { Name = _l.Loc("Users"), Url = "/users", Icon = "mlx-icon-menu mlx-ico-users-group-light" }
					}
                }},
				{ "organization", new MenuItem
					{
						Name = _l.Loc("Organization"),
						Icon = "mlx-icon-menu mlx-ico-organization-light",
						Url = "/organization"
					}
				},
				{ "warehouses", new MenuItem
					{
						Name = _l.Loc("Warehouses"),
						Icon = "mlx-icon-menu mlx-ico-warehouse-light",
						Url = "/warehouses"
					}
				},
				{ "users", new MenuItem
					{
						Name = _l.Loc("Users"),
						Icon = "mlx-icon-menu mlx-ico-users-group-light",
						Url = "/users"
					}
				},
				//Menu MANAGMENT
				{"management" , new MenuItem{

					Name = _l.Loc("GENERAL CONFIGURATION"),
					IsParent = true,
					BreadcrumbChildren = new List<MenuItem>
					{
						new() { Name = _l.Loc("Work profiles"), Url = "/work-profile", Icon = "mlx-icon-menu mlx-ico-workers-light"},
						new() { Name = _l.Loc("Logistics profiles"), Url = "/logistics-profile", Icon = "mlx-icon-menu mlx-ico-data-source-light"},
						new() { Name = _l.Loc("Alerts"), Url = "/alerts", Icon = "mlx-icon-menu mlx-ico-alert-management-light"},
                        new() { Name = _l.Loc("Parameters"), Url = "/parameters", Icon = "mlx-icon-menu mlx-ico-parameters-light"},
                        new() { Name = _l.Loc("TransactionLog"), Url = "/transactionLog", Icon = "mlx-icon-menu mlx-ico-exchange"},
                    }

				}},				
				{ "work-profile", new MenuItem
					{
						Name = _l.Loc("Work profiles"),
						Icon = "mlx-icon-menu mlx-ico-workers-light",
						Url = "/work-profile",
						HasSubMenu = true,
						SubMenuItems = new List<MenuItem>
						{
							new() { Name = _l.Loc("Base profiles"), Url = "/work-profile", TabKey = "tab1",  DependsOnWarehouse = true, },
							new() { Name = _l.Loc("Operators profiles"), Url = "/work-profile", TabKey = "tab2",  DependsOnWarehouse = true, },
							new() { Name = _l.Loc("Equipment profiles"), Url = "/work-profile", TabKey = "tab3",  DependsOnWarehouse = true, },
						},
						BreadcrumbChildren = new List<MenuItem>
						{
                            new() { Name = _l.Loc("Base profiles"), Url = "/work-profile", TabKey = "tab1",  DependsOnWarehouse = true, },
                            new() { Name = _l.Loc("Operators profiles"), Url = "/work-profile", TabKey = "tab2",  DependsOnWarehouse = true, },
                            new() { Name = _l.Loc("Equipment profiles"), Url = "/work-profile", TabKey = "tab3",  DependsOnWarehouse = true, },
                        }
					}
				},
				{ "logistics-profile", new MenuItem
					{
						Name = _l.Loc("Logistics profiles"),
						Icon = "mlx-icon-menu mlx-ico-data-source-light",
						Url = "/logistics-profile",
						HasSubMenu = true,
						SubMenuItems = new List<MenuItem>
						{
							new() { Name = _l.Loc("Base profiles"), Url = "/logistics-profile", TabKey = "tab1", DependsOnWarehouse = true, },
							new() { Name = _l.Loc("Order profiles"), Url = "/logistics-profile", TabKey = "tab2", DependsOnWarehouse = true, },
							new() { Name = _l.Loc("Load profiles"), Url = "/logistics-profile", TabKey = "tab3",  DependsOnWarehouse = true, },
						},
						BreadcrumbChildren = new List<MenuItem>
						{
                            new() { Name = _l.Loc("Base profiles"), Url = "/logistics-profile", TabKey = "tab1", DependsOnWarehouse = true, },
                            new() { Name = _l.Loc("Order profiles"), Url = "/logistics-profile", TabKey = "tab2", DependsOnWarehouse = true, },
                            new() { Name = _l.Loc("Load profiles"), Url = "/logistics-profile", TabKey = "tab3",  DependsOnWarehouse = true, },
                        }
					}
				},
				{ "alerts", new MenuItem
					{
						Name = _l.Loc("Alerts"),
						DependsOnWarehouse = true,
						Icon = "mlx-icon-menu mlx-ico-alert-management-light",
						Url = "/alerts"
					}
				},
				{ "parameters", new MenuItem
					{
						Name = _l.Loc("Parameters"),
						DependsOnWarehouse = true,
						Icon = "mlx-icon-menu mlx-ico-parameters-light",
						Url = "/parameters",
						HasSubMenu = true,
                        SubMenuItems = new List<MenuItem>
                        {
                            new() { Name = _l.Loc("Priority of execution"), Url = "/parameters", TabKey = "tab1", DependsOnWarehouse = true, },
                            new() { Name = _l.Loc("SLA"), Url = "/parameters", TabKey = "tab2", DependsOnWarehouse = true, },
                        },
                        BreadcrumbChildren = new List<MenuItem>
                        {
                            new() { Name = _l.Loc("Priority of execution"), Url = "/parameters", TabKey = "tab1", DependsOnWarehouse = true, },
                            new() { Name = _l.Loc("SLA"), Url = "/parameters", TabKey = "tab2", DependsOnWarehouse = true, },
                        }
                    }
				},
                { "transactionLog", new MenuItem
                    {
                        Name = _l.Loc("TransactionLog"),
                        DependsOnWarehouse = true,
                        Icon = "mlx-icon-menu mlx-ico-exchange",
                        Url = "/transactionLog"
                    }
                },
            };
		}
		#endregion

		#region Methods
		public MenuItem? GetMenuItem(string key) =>
			_navigationItems.TryGetValue(key, out var value) ? value : null;

		public List<MenuItem> GetMenuItems() => _navigationItems.Values.ToList();
		#endregion
	}
}