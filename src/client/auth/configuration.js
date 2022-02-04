import {WebStorageStateStore} from "oidc-client";
import {ApplicationPaths, GigyaOidcClient} from "./ApiAuthorizationConstants";
import withBasePath from "../publicPath";
// const configuration = {
//   client_id: GigyaOidcClient.ClientId,
//   client_secret: GigyaOidcClient.ClientSecret,
//   redirect_uri: withBasePath('authentication/callback'),
//   response_type: 'code',
//   post_logout_redirect_uri: withBasePath(''),
//   scope: 'openid profile email api offline_access',
//   authority: GigyaOidcClient.Authority,
//   silent_redirect_uri: withBasePath('authentication/silent_callback'),
//   automaticSilentRenew: true,
//   loadUserInfo: true,
// };
// const configuration = {
//   client_id: GigyaOidcClient.ClientId,
//   client_secret: GigyaOidcClient.ClientSecret,
//   display: 'popup',
//
//   redirect_uri: ApplicationPaths.LoginCallback,
//   response_type: 'code',
//   post_logout_redirect_uri: ApplicationPaths.LogOutCallback,
//   scope: 'openid profile email api offline_access',
//   authority: GigyaOidcClient.Authority,
//   silent_redirect_uri: ApplicationPaths.SilentLoginCallback,
//   automaticSilentRenew: true,
//   loadUserInfo: true,
//   includeIdTokenInSilentRenew: true,
//   userStore: new WebStorageStateStore({store: window.localStorage})
//
// };

const configuration = {
  client_id: GigyaOidcClient.ClientId,
  client_secret: GigyaOidcClient.ClientSecret,
  redirect_uri: withBasePath('authentication/callback'),
  response_type: 'code',
  post_logout_redirect_uri: withBasePath(''),
  scope: 'openid profile email api offline_access',
  authority: GigyaOidcClient.Authority,
  silent_redirect_uri: withBasePath('authentication/silent_callback'),
  automaticSilentRenew: true,
  loadUserInfo: true,
  display: 'popup',
};

console.log(configuration);

export default configuration;
