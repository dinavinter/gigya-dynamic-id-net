import * as React from 'react';
import JSONTree from 'react-json-tree';



export default function JsonTreeViewer  ({data, title, shouldExpandNode})  {


    if(!data)
        return <div>loading</div>
    return  (
      <>
        <h1>{title}</h1>
        <JSONTree data={data} theme="bright" shouldExpandNode={shouldExpandNode} />
      </>
    ) ;
  }

