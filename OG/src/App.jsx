import { useState, useEffect } from 'react'
import './App.css'
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import * as signalR from '@microsoft/signalr';
import MainPage from './component/MainPage.jsx';
import LobbyPage from './component/LobbyPage.jsx';
import { NotFoundPage } from './component/NotFoundPage.jsx';
import GamePage from './component/GamePage.jsx';
import VideoRecorderPage from './component/VideoRecorderPage.jsx';

const EVENT_PARTY_CREATED = "PartyCreated";
const EVENT_PARTY_JOINED = "PartyJoined";

function App() {

  const [players, setPlayers] = useState([]);
  const [messages, setMessages] = useState([]);
  const [connection, setConnection] = useState(null);

  useEffect(() => {
    const initializeConnection = async () => {
      if (!connection) {
        const newConnection = new signalR.HubConnectionBuilder()
          .withUrl('http://10.30.90.94:5000/hub')
          .withAutomaticReconnect()
          .build();

        newConnection.on("ReceivePlayers", (receivedPlayers) => {
          setPlayers(receivedPlayers);
        });

        try {
          await newConnection.start();
          setConnection(newConnection);
        } catch(error) {
          console.error('Error starting connection: ', error);
        }
      }
    }

    initializeConnection();

    return () => {
      if (connection) {
        connection.stop();
      }
    };
  }, [connection])

  const joinParty = async (nav, partyId, username) => {
    connection.off(EVENT_PARTY_JOINED);

    connection.on(EVENT_PARTY_JOINED, (_) => {
      console.log("Party joined: " + partyId);
      nav("/lobby/" + partyId);
    });

    try {
      await connection.invoke("JoinParty", partyId, username);
    } catch (error) {
      console.error('Error joining party:', error);
    }
  }

  const createParty = async (nav, username) => {
    connection.off(EVENT_PARTY_CREATED);

    connection.on(EVENT_PARTY_CREATED, (partyId) => {
      console.log("Party created: " + partyId);
      nav("/lobby/" + partyId);
    });

    try {
      await connection.invoke("CreateParty", username);
    } catch (error) {
      console.error('Error creating party:', error);
    }
  }

  return (
    <Router>
      <Routes>
        <Route path="/" element={<MainPage joinParty={joinParty} createParty={createParty} />}/>
        <Route path="/video" element={<VideoRecorderPage />}/>
        <Route path="/lobby/:id" element={<LobbyPage connection={connection} players={players}/>}/>
        <Route path="/game/:id" element={<GamePage messages={messages} />}/>
        <Route path="/404" element={<NotFoundPage/>}/>
        <Route path="*" element={<Navigate to="/" />} />
      </Routes>
    </Router>
  )
}

export default App