import * as React from 'react';

import {ToastContainer} from 'react-toastify';
import  { useApi} from '../services/ApiService';

import AuthContent from './AuthContent';
import Buttons from './Buttons';
import {useReactOidc} from "@axa-fr/react-oidc-context";


function AppContent() {

    const { oidcUser  } = useReactOidc();
    const {api, data} = useApi();


    return (
        <>
            <ToastContainer/>

            <AuthContent api={data} user={oidcUser}/>
        </>
    );
}

export default AppContent;
