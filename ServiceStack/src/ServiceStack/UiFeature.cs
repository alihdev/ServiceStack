using System;
using System.Collections.Generic;
using ServiceStack.Admin;
using ServiceStack.Configuration;
using ServiceStack.DataAnnotations;
using ServiceStack.HtmlModules;
using ServiceStack.Model;

namespace ServiceStack;

[Flags]
public enum AdminUiFeature
{
    None       = 0,
    Users      = 1 << 0,
    Validation = 1 << 1,
    Logging    = 1 << 2,
    Profiling  = 1 << 3,
    Redis      = 1 << 4,
    Database   = 1 << 5,
    All = Users | Validation | Logging | Profiling | Redis | Database,
}

public class UiFeature : IPlugin, IPreInitPlugin, IPostInitPlugin, IHasStringId
{
    public string Id => Plugins.Ui;

    public UiInfo Info { get; set; }

    public List<HtmlModule> HtmlModules { get; } = new();
    
    public HtmlModule AdminHtmlModule { get; set; } = new("/modules/admin-ui", "/admin-ui") {
        DynamicPageQueryStrings = { nameof(MetadataApp.IncludeTypes) }
    };
    public AdminUiFeature AdminUi { get; set; } = AdminUiFeature.All;

    /// <summary>
    /// Links to make available to users in different roles (e.g. in built-in UIs) 
    /// </summary>
    public Dictionary<string, List<LinkInfo>> RoleLinks { get; set; } = new();

    public LinkInfo DashboardLink { get; set; } = new()
    {
        Id = "",
        Label = "Dashboard",
        Icon = Svg.ImageSvg(Svg.Create(Svg.Body.Home)),
    };

    public List<IHtmlModulesHandler> Handlers { get; set; } = new()
    {
        new SharedFolder("shared", "/modules/shared", ".html"),
        new SharedFolder("shared/js", "/modules/shared/js", ".js"),
        new SharedFolder("plugins", "/modules/shared/plugins", ".js"),
    };

    public HtmlModulesFeature Module { get; } = new HtmlModulesFeature {
            IgnoreIfError = true,
        }
        .Configure((appHost,module) => 
            module.VirtualFiles = appHost.VirtualFileSources);
    
    public Action<IAppHost> Configure { get; set; }

    /// <summary>
    /// Only Attributes used in built-in UIs are returned in /metadata/app.json  
    /// </summary>
    public List<string> PreserveAttributesNamed { get; set; } = new()
    {
        nameof(ComputedAttribute),
    };

    public UiFeature()
    {
        Info = new UiInfo
        {
            HideTags = new() { TagNames.Auth, TagNames.Admin },
            AlwaysHideTags = new() { TagNames.Admin },
            BrandIcon = Svg.ImageUri(Svg.GetDataUri(Svg.Logos.ServiceStack, "#000000")),
            Theme = new ThemeInfo
            {
                Form = "shadow overflow-hidden sm:rounded-md bg-white",
                ModelIcon = Svg.ImageSvg(Svg.Create(Svg.Body.Table)),
            },
            DefaultFormats = new ApiFormat
            {
                // Defaults to browsers navigator.languages
                //Locale = Thread.CurrentThread.CurrentCulture.Name,
                AssumeUtc = true,
                Date = new Intl(IntlFormat.DateTime) {
                    Date = DateStyle.Medium,
                }.ToFormat(),
            },
            Locode = new()
            {
                Css = new ApiCss
                {
                    Form = "max-w-screen-2xl",
                    Fieldset = "grid grid-cols-12 gap-6",
                    Field = "col-span-12 lg:col-span-6 xl:col-span-4",
                },
                Tags = new AppTags
                {
                    Default = "Tables",
                    Other = "other",
                },
                MaxFieldLength = 150,
                MaxNestedFields = 2,
                MaxNestedFieldLength = 30,
            },
            Explorer = new()
            {
                Css = new ApiCss
                {
                    Form = "max-w-screen-md",
                    Fieldset = "grid grid-cols-12 gap-6", 
                    Field = "col-span-12 sm:col-span-6",
                },
                Tags = new AppTags
                {
                    Default = "APIs",
                    Other = "other",
                },
            },
            Admin = new()
            {
                Css = new ApiCss
                {
                    Form = "max-w-screen-lg",
                    Fieldset = "grid grid-cols-12 gap-6", 
                    Field = "col-span-12",
                }
            },
            AdminLinks = new(),
        };
    }
    
    public void AddAdminLink(AdminUiFeature feature, LinkInfo link)
    {
        if (!AdminUi.HasFlag(feature)) 
            return;
        
        if (!RoleLinks.TryGetValue(RoleNames.Admin, out var roleLinks))
            roleLinks = RoleLinks[RoleNames.Admin] = new();
        roleLinks.Add(link.ToAdminRoleLink());

        Info.AdminLinks.Add(link);
    }

    public void BeforePluginsLoaded(IAppHost appHost)
    {
        if (AdminHtmlModule != null && AdminUi != AdminUiFeature.None)
        {
            HtmlModules.Add(AdminHtmlModule);
            AddAdminLink(AdminUiFeature.None, DashboardLink);
            appHost.RegisterService(typeof(AdminDashboardService));
        }
    }

    public void Register(IAppHost appHost)
    {
    }

    public void AfterPluginsLoaded(IAppHost appHost)
    {
        if (HtmlModules.Count > 0)
        {
            Info.Modules = HtmlModules.Map(x => x.BasePath);
            Configure?.Invoke(appHost);
            Module.Modules.AddRange(HtmlModules);
            Module.Handlers.AddRange(Handlers);
            Module.Register(appHost);
        }
    }
}

public static class UiFeatureUtils
{
    public static LinkInfo ToAdminRoleLink(this LinkInfo link) => new() {
        Id = link.Id,
        Label = link.Label,
        Icon = link.Icon,
        Href = "../admin-ui" + (string.IsNullOrEmpty(link.Id) ? "" : "/" + link.Id),
        Show = link.Show,
    };
}
