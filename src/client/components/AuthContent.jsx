import * as React from 'react';
import JsonTreeViewer from './JsonTreeViewer';


export default class AuthContent extends React.Component  {
    shouldExpandNode = (keyPath  , data , level ) => {
    return true;
  };

    render() {
    return (
      <div className="row">
        <div className="col-md-6">
          <JsonTreeViewer data={this.props.user} title="User Profile" shouldExpandNode={this.shouldExpandNode} />
        </div>
        <div className="col-md-6">
          <JsonTreeViewer data={this.props.api} title="Api Response" shouldExpandNode={this.shouldExpandNode} />
        </div>
      </div>
    );
  }
}
