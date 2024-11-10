import ReactPlayer from "react-player";
import './App.css';
import { useState } from "react";

declare global {
    let openFilePicker: () => Promise<string | null>;
}

function App() {
    const [playFilePath, setPlayFilePath] = useState("");

    async function pickButtonClicked() {
        const filePath = await openFilePicker();
        if (filePath == null) {
            console.log("Failed opening file");
            return;
        }

        setPlayFilePath("");

        setTimeout(() => {
            setPlayFilePath(filePath);
        }, 100);
    }

    return (
        <div className="app">
            { playFilePath && <ReactPlayer controls url={playFilePath}/> }

            <button className="pick-button" onClick={pickButtonClicked}>
                <p className="pick-button-text">Open Video</p>
            </button>
        </div>
    );
}

export default App;
