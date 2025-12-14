import './App.css';
import GlassLogin from './GlassLogin';
import Chat from './Chat';
import { useState } from 'react';

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  return (
    <>
      {isLoggedIn ? <Chat /> : <GlassLogin onLoginSuccess={() => setIsLoggedIn(true)} />}
    </>
  );
}

export default App;