import React from 'react';
import logo from './logo.svg';
import './App.css';
import {VoteView} from "./Views/VoteView";
import {CreateElectionView} from "./Views/CreateElectionView";

function App() {
  return (
    <div className="App">
        <header className={"App-header"}></header>
      {/*<VoteView/>*/}
        <CreateElectionView/>
    </div>
  );
}

export default App;
