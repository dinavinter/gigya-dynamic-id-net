import React from 'react';
import './ie.polyfills';

 import { BrowserRouter as Router } from 'react-router-dom';
import { AuthenticationProvider, oidcLog, InMemoryWebStorage } from '@axa-fr/react-oidc-context';
import CustomCallback from './pages/CustomCallback';
import Header from './layout/Header';
import oidcConfiguration from './auth/configuration';
import Routes from "./router/Router";
import {GigyaOidcClient} from "./auth/ApiAuthorizationConstants";
import  withBasePath from './publicPath'
import Sidebar from "./layout/SideBar";
import Layout from "./layout/Layout";

const App = () => {

    return <div>
        <AuthenticationProvider
            configuration={oidcConfiguration}
            loggerLevel={oidcLog.DEBUG}
            isEnabled={true}
            callbackComponentOverride={CustomCallback}
            UserStore={InMemoryWebStorage}
        >
            <Router>
             <Layout>
                <Routes />
             </Layout>
            </Router>
        </AuthenticationProvider>
    </div>;

}
export default App;
