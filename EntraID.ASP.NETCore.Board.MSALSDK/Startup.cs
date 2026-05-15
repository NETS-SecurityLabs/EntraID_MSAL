using EntraID.ASP.NETCore.Board.Business;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;

namespace EntraID.ASP.NETCore.Board
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// ASP.NET Core 인증을 OpenIdConnect 인증 방식으로 설정하기 위해서 AddAuthentication과
			// AddMicrosoftIdentityWebApp을 호출한다.
			services.AddAuthentication(Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme) // 인증 방식으로 OpenidConnect를 사용
				.AddMicrosoftIdentityWebApp(idOptions =>
				{
					// 테넌트 ID를 설정한다.
					idOptions.TenantId = Configuration.GetValue<string>("EntraID:TenantID");
					// Entra 관리센터에서 등록한 앱의 ID를 설정한다.
					idOptions.ClientId = Configuration.GetValue<string>("EntraID:ClientID");
					// Entra ID 인증 기관의 인스턴스 URI를 설정한다. 
					idOptions.Instance = Configuration.GetValue<string>("EntraID:Instance");
				});

			services.AddControllersWithViews();			
			// 앱의 게시판 기능 추가
			services.AddSingleton<BoardManager>(new BoardManager(Configuration.GetValue<string>("BoardData")));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			var forwardedHeadersOptions = new ForwardedHeadersOptions
			{
				ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
			};
			forwardedHeadersOptions.KnownIPNetworks.Clear();
			forwardedHeadersOptions.KnownProxies.Clear();
			app.UseForwardedHeaders(forwardedHeadersOptions);

			app.UseDeveloperExceptionPage();
			app.UseStaticFiles();

			app.UseRouting();
			// ASP.NET Core Identity Framework의 인증 미들 웨어를 추가한다.
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Board}/{action=Index}/{id?}");
			});

			// 사용자 정보를 초기화한다.
			UserManager.Initialize(Configuration.GetValue<string>("UserInfo"));
		}
	}
}
