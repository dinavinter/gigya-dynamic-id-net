import {useReactOidc} from "@axa-fr/react-oidc-context";
import {toast} from "react-toastify";
import {useState} from "react";

export function useToken() {
  const {signinSilent, oidcUser} = useReactOidc();

  if (oidcUser && oidcUser.access_token) {
    return oidcUser.access_token;
  } else if (oidcUser) {
    signinSilent();
    return oidcUser.access_token;
  } else return null;


}


export function useApi() {
  const token = useToken();
  const [apiData, setApiData] = useState();
  return {
    api: () => callApi(token)
      .then(data => {
        setApiData(data.data);
        toast.success('Api return successfully data, check in section - Api response');
      })
      .catch(error => {
        toast.error(error);
      }),
    data:apiData
  }

  async function callApi(token) {
    const response = await fetch('/api/values', {
      headers: !token ? {} : {'Authorization': `Bearer ${token}`}
    });

    return await response.json();

  }

}








