import './LandingPage.css'
import '../Common/common.css'
import { Link } from "react-router-dom";
import {v4} from "uuid";

export function LandingPage() {
    return <div className={"landing-page"}>
            <div className={"box landing-box"}>
            <h2>Welcome to my ranked choice voting app!</h2>

            <Link to={"/election/"+v4()}>Click here to start a new election</Link>
        </div>
    </div>
}