using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;

namespace Notes.Identity;

/// <summary>
/// данный класс содержит информацию о клиентах, ресурсах, пользователях и т.д.
/// </summary>
public static class Configuration
{
    /// <summary>
    /// Scopes или области представляет то, что клиентскому приложению можно использовать, т.е. идентификатор
    /// ресурса. Этот идентификатор отправляется в процессе аутентификации или запроса токена. Об этом можно думать 
    /// как о доступе на уровне области. В Identity server области представлены ресурсами, это могут быть Identity
    /// и Api ресурсы
    /// </summary>
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("NotesWebAPI", "Web API")
        };

    /// <summary>
    /// IdentityResource позволяет смоделировать область, которая позволит клиентскому приложению просматривать
    /// подмножество утверждени о пользователе
    /// </summary>
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            //область Profile позволяет видеть приложению утверждения о пользователе, такие как например имя и дата рождения
            //утверждения еще называются клэймами
            new IdentityResources.Profile()
        };

    /// <summary>
    /// ApiResource позволяет смоделировать доступ ко всему защищенному ресурсу.
    /// Api с отдельными уровнями разрешений или областями, к которым клиентское приложение может запросить доступ
    /// В нашем случае назовем ApiResource "NotesWebAPI" и Scopes = {"NotesWebAPI"}
    /// </summary>
    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            //в массив Claim или утверждений передадим Name
            new ApiResource("NotesWebAPI", "Web API", new []
                {JwtClaimTypes.Name})
            {
                Scopes = {"NotesWebAPI"}
            }
        };

    /// <summary>
    /// Identity server необходимо знать каким клиентским приложениям позволено использовать его. Мы можем думать о
    /// клиентах как о списке приложений, которые могут использовать нашу систему, каждое клиентское приложение
    /// конфигурируется так, что ему позволено делать только определенный набор вещей
    /// </summary>
    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new Client
            {
                ClientId = "notes-web-api",//идентификатор клиента (в конфигурации клиента должен быть такой же)
                ClientName = "Notes Web",

                //OAuth 2.0 и OIDC определяют так называемые грант типы, которые определяют как клиент взаимодействует
                //с сервисом токена
                //OAuth 2.0 опред. несколько грант типов для разных сценариев использования
                //наиболее распространенные следующие:
                //
                //Authorization Code - используется конфеденциальными и публичными клиентами для обмена кода авторизации на токен доступа
                //после того как пользователь вернется к клиенту по URL адресу перенаправления, приложение получит код авторизации из URL адреса
                //и использует его для запроса токена доступа
                //
                //Client Credentials - используется клиентами для получения токена доступа вне контекста пользователя, обычно используется
                //клиентами для доступа к ресурсам о себе, а не для доступа к пользовательским ресурсам
                //
                //Device Code- используется устройствами без браузера или устройствами с ограничением ввода в Device flow для обмена
                //ранее полученного кода устройства на токен доступа. Device flow это расширение OAuth 2.0, которое и позволяет работать
                //с упомянутыми типами устройств
                //
                //Refresh Token - этот тип используется клиентами для обмена токена обновления на токен доступа, когда срок действия предыдущего токена 
                //доступа истек. Это позволяет клиентам продолжать иметь действительный токен доступа без дальнейшего взаимодействия
                //с пользователем
                AllowedGrantTypes = GrantTypes.Code,//Authorization Code
                RequireClientSecret = false,//false т.к. мы не будем использовать секрет клиента
                RequirePkce = true,//говорит о том, что нужен ключ подтверждения для Authorization Code
                RedirectUris = //это набор Uri адресов, куда может происходить перенаправление после аутентификации клиентского приложения
                {
                    "http://localhost:3000/signin-oidc"//после базового адреса клиента (который мы пока не знаем) должен присутствовать signin-oidc
                },
                AllowedCorsOrigins = //набор Uri адресов, кому позволено использовать Identity server
                {
                    "http://localhost:3000"
                },
                PostLogoutRedirectUris = //набор Uri адресов, куда может происходить перенаправление после выхода клиентского приложения
                {
                    "http://localhost:3000/signout-oidc"//после базового адреса клиента (который мы пока не знаем) должен присутствовать signout-oidc
                },
                AllowedScopes = //это области или Scopes, доступные клиенту
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "NotesWebAPI"
                },
                AllowAccessTokensViaBrowser = true//управляет передачей токена доступа через браузер(по умолчанию false)
            }
        };

}
