import { useParams } from "react-router";
import { MessageContainer } from "./MessageContainer";

const GamePage = ( {messages} ) => {

    const { id } = useParams();

    return (
        <div>
            <MessageContainer key={0} msg={"Hello world"}/>
            {messages.map((msg, index) => (
                <MessageContainer key={index} msg={msg}/>
            ))}
        </div>
    );
}

export default GamePage;