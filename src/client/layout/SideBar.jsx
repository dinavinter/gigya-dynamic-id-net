import React from 'react'
import {useReactOidc} from "@axa-fr/react-oidc-context";
import Buttons from "../components/Buttons";
import  "./sideBar.css";
import '../index.css'

function Sidebar() {
    const {isEnabled, login, logout, signinSilent, oidcUser} = useReactOidc();

    return  <div className="sidenav">
        <Buttons

            login={login}
            logout={logout}
            renewToken={signinSilent}
            getUser={signinSilent}
        />
    </div>
 }

export default Sidebar
