using EntraID.ASP.NETCore.Board.Business;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using EntraID.ASP.NETCore.Board.Controllers;
using Microsoft.AspNetCore.Http;

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
			services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme) // 인증 방식으로 OpenidConnect를 사용
				.AddMicrosoftIdentityWebApp(idOptions =>
				{
					// 테넌트 ID를 설정한다.
					idOptions.TenantId = Configuration.GetValue<string>("EntraID:TenantID");
					// Entra 관리센터에서 등록한 앱의 ID를 설정한다.
					idOptions.ClientId = Configuration.GetValue<string>("EntraID:ClientId");
					// Entra ID 인증 기관의 인스턴스 URI를 설정한다. 
					idOptions.Instance = Configuration.GetValue<string>("EntraID:Instance");
					// 리디렉션 URI를 설정한다.
					idOptions.CallbackPath = Configuration.GetValue<string>("EntraID:CallbackPath");
					// 샘플 웹에서 가장 먼저 로그아웃을 Entra ID에 요청했을 경우에, QueryString으로 전달되는 URL을 생성하기 위해서
					// 사용되는 설정이다. 이 설정을 이용해서 ASP.NET Core MSAL SDK는 전체 URL을 완성헤서, 로그아웃 요청시 함께 보낸다.
					// Entra ID는 인증되어 있는 모든 웹 앱을 로그아웃 시킨 후에 마지막으로 사용자의 브라우저를 이동시킬 URL로 이 설정을 사용한다.
					idOptions.SignedOutCallbackPath = Configuration.GetValue<string>("EntraID:PostSignoutCallbackPath");
					// 인증 요청시에 보낼 권한 정보를 설정한다.
					idOptions.Scope.Add(OpenIdConnectScope.Email);
					// Entra ID가 사용자 인증을 완료한 후에 반환할 인증 정보 유형을 설정한다.
					idOptions.ResponseType = OpenIdConnectResponseType.IdToken;
					// Entra ID와 MSAL SDK가 사용자 인증을 처리하는 동안 오류가 발생할 경우에, 해당 내용을 사용자에게 커스텀 
					// 페이지를 이용해서 제공할 수 있는 기회를 갖기 위해서 인증 실패 이벤트 처리기를 설정한다.
					idOptions.Events.OnAuthenticationFailed = MsalAuthenticationController.OnAuthenticationFailed;
				},

				// ASP.NET Core Identity Framework에서 관리하는 인증 정보 쿠키 발급 정책을 설정한다.
				cookieOptions =>
				{
					cookieOptions.Cookie = new CookieBuilder
					{
						HttpOnly = true,
						// Entra ID를 로그아웃을 진행할 때, IFrame을 이용해서 인증을 받은 대상 시스템에게 
						// 로그아웃 URL을 호출한다. 로그아웃 요청을 받은 대상 시스템은 사용자 인증 정보를
						// 저장하고 있는 쿠키를 삭제해야 하는데, 이 경우에 브라우저의 SameSite 정책에 따라서
						// 쿠키 삭제가 되지 않을 수 있다. 이런 현상에 대응하기 위해서 SameSite 설정을 None으로
						// 설정해 준다. 
						SameSite = SameSiteMode.None,
						SecurePolicy = CookieSecurePolicy.Always
					};
				});

			services.AddControllersWithViews();

			
			// BoardManager를 싱글톤으로 등록한다.
			services.AddSingleton<BoardManager>(new BoardManager(Configuration.GetValue<string>("BoardData")));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
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
