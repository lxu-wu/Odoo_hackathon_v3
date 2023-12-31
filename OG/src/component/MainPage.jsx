import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useNavigate } from 'react-router-dom'

const MainPage = ({ joinParty, createParty }) => {

  const nav = useNavigate();

  const _joinParty = () => {
    const username = document.getElementById("pseudo_join").value;
    const party_id = document.getElementById("code_party").value;

    joinParty(nav, party_id, username);
  };

  const _createParty = () => {
    const username = document.getElementById("pseudo_create").value;
    createParty(nav, username);
  };

  return (
    <>
      <div className='card'>
        <div >
          <img className='logo' src="../public/olymp-logo.jpg" alt="" />
        </div>
        <div className="row">
          <div id="join-link">
            <img src="../public/ekip7.png" alt="" id="groupe-logo" />
            <div>
              <input type="text" placeholder='Pseudo' id='pseudo_join' className='pseudo'/>
            </div>
            <div>
              <input type="text" placeholder="Code de votre partie" id="code_party" /><div id="div-code-partie">

              </div>

              <button id='join' onClick={_joinParty}>Rejoindre</button>

            </div>
          </div>

          <div id="tournoie-link">
            <img src="../public/start2.jpeg" alt="" id="start" />

            <input type="text" placeholder='Pseudo' id='pseudo_create' className='pseudo'/>

            <div>
              <button id='join' onClick={_createParty}>Créer un Tournoi</button>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}

export default MainPage;