import './LandingPage.css'
import '../Common/common.css'
import {v4} from "uuid";
import {useCookies} from "react-cookie";
import {createElection} from "../Common/ElectionModel";
import {useState} from "react";
import { Navigate } from 'react-router-dom';


export function LandingPage() {

    const [state, setState] = useState({electionId: ""})

    const [cookies, setCookie] = useCookies(['userId'])
    if (cookies.userId===undefined)
    {
        setCookie('userId', v4())
    }

    return <div className={"landing-page"}>
            <div className={"box landing-box"}>
                <h2>Welcome to my ranked choice voting app!</h2>
                {state.electionId != "" && <Navigate to={"/election/" + state.electionId}/>}

                <a href={"#"} onClick={async e => {
                    var electionResponse = await createElection(cookies.userId)
                    if (electionResponse.response != null)
                    {
                        var guid = electionResponse.response
                        setState(state => {
                            return {...state, electionId: guid}}
                        )
                    }
                }}>Click here to start a new election</a>

        </div>
    </div>
}