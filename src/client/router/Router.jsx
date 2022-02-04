import {OidcSecure, withOidcSecure} from "@axa-fr/react-oidc-context";
import {Switch , Route} from "react-router-dom";
import React from "react";
import Dashboard from "../pages/Dashboard";
import SimpleComponent from "../pages/SimpleComponnet";
import Admin from "../pages/Admin";
import Home from "../components/AppContent";
import AppContent from "../components/AppContent";

const PageNotFound = () => <div>Page not found</div>;
const ProtectedSimpleComponent = withOidcSecure(SimpleComponent);
const ProtectedDashboard = withOidcSecure(Dashboard);

const Router = () => (
    <Switch >
        <Route exact path="/">
            <AppContent />
        </Route>
        <Route path="/dashboard">
            <ProtectedDashboard />
        </Route>
        <Route path="/admin">
            <OidcSecure>
                <Admin />
            </OidcSecure>
        </Route>
        <Route path="/home" component={Home} />
        <Route path="/protected1">
            <OidcSecure>
                <SimpleComponent type="Component" />
            </OidcSecure>
        </Route>
        <Route path="/protected2">
            <ProtectedSimpleComponent type="HOC" />
        </Route>
        <Route component={PageNotFound} />
    </Switch >
);

export default Router;