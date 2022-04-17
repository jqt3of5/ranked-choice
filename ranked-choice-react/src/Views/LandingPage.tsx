import './LandingPage.css'
import '../Common/common.css'
import { Link } from "react-router-dom";
import {v4} from "uuid";
import {useCookies} from "react-cookie";
import {createElection} from "../Common/ElectionModel";
import {useEffect, useState} from "react";

export function LandingPage() {

    const [state, setState] = useState({electionId: ""})

    useEffect()
    const [cookies, setCookie] = useCookies(['userId'])
    if (cookies.userId===undefined)
    {
        setCookie('userId', v4())
    }
    return <div className={"landing-page"}>
            <div className={"box landing-box"}>
            <h2>Welcome to my ranked choice voting app!</h2>

            <Link onClick={e => {
                createElection(cookies.userId).then(guid => {

                })
            }}>Click here to start a new election</Link>
        </div>
    </div>
}