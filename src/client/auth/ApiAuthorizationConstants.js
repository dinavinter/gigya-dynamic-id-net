export const ApplicationName = 'gigya-id-web';

export const QueryParameterNames = {
  ReturnUrl: 'returnUrl',
  Message: 'message'
};

export const LogoutActions = {
  LogoutCallback: 'logout-callback',
  Logout: 'logout',
  LoggedOut: 'logged-out'
};

export const LoginActions = {
  Login: 'login',
  LoginCallback: 'login-callback',
  LoginFailed: 'login-failed',
  Profile: 'profile',
  Register: 'register',
  SilentCallback: 'silent_callback'
};

export const GigyaOP = {
  DataCenter: "us1",
  ApiKey: "3__NKd98KtcRCL_Z98TO7bbTtMhZqe83A4hOjMA2wblxL8PAoduwgW9FTvdQ6OqYIB",

}

export var GigyaOidcClient = {
   Authority: `https://fidm.us1.gigya.com/oidc/op/v1.0/3__NKd98KtcRCL_Z98TO7bbTtMhZqe83A4hOjMA2wblxL8PAoduwgW9FTvdQ6OqYIB`,
   ClientId: 'AHwiThd52-bPTYYXq0UFOosC',
   ClientSecret:'poVgJrnr3fn7bxq-fVkabe-EDXPYmREK3AX1alWO3UwgLb4BeJ-dALONF5xJhFRXZRZn9QT79V-vK6KICyGcxg'
}

const prefix = '/authentication';

export const ApplicationPaths = {
  DefaultLoginRedirectPath: '/',
  ApiAuthorizationClientConfigurationUrl: `/_configuration/${ApplicationName}`,
  ApiAuthorizationPrefix: prefix,
  Login: `${prefix}/${LoginActions.Login}`,
  LoginFailed: `${prefix}/${LoginActions.LoginFailed}`,
  LoginCallback: `${prefix}/${LoginActions.LoginCallback}`,
  SilentLoginCallback: `${prefix}/${LoginActions.SilentCallback}`,
  Register: `${prefix}/${LoginActions.Register}`,
  Profile: `${prefix}/${LoginActions.Profile}`,
  LogOut: `${prefix}/${LogoutActions.Logout}`,
  LoggedOut: `${prefix}/${LogoutActions.LoggedOut}`,
  LogOutCallback: `${prefix}/${LogoutActions.LogoutCallback}`,
  IdentityRegisterPath: '/Identity/Account/Register',
  IdentityManagePath: '/Identity/Account/Manage'
};
