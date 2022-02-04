import React from 'react';
import {useReactOidc} from '@axa-fr/react-oidc-context';
import {Link} from 'react-router-dom';
import Buttons from "../components/Buttons";
import '../index.css'
const headerStyle = {
  // display: 'flex',
  // backgroundColor: '#26c6da',
  // justifyContent: 'space-between',
  // padding: 10,
};

const linkStyle = {
  // color: 'white',
  // textDecoration: 'underline',
};

const Header = () => {
  return (
    <header>

      <div style={headerStyle}>


        <ul className={"breadcrumb"}>
          <li>
            <Link style={linkStyle} to="/">
              Home
            </Link>
          </li>
          <li>
            <Link style={linkStyle} to="/dashboard">
              Dashboard
            </Link>
          </li>
          <li>
            <Link style={linkStyle} to="/admin">
              Admin
            </Link>
          </li>
        </ul>

      </div>
    </header>
  );
};

export default Header;
