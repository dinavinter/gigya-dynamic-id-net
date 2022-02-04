import Header from "./Header";
import Sidebar from "./SideBar";
import React from "react";
import '../index.css'

export default function  Layout({children}){
    return <div>
        <Sidebar />
        <Header />
        <div  className="content"  class="content">

            {children}
        </div>
    </div>
}