import { useState } from "react";
import { NavLink, Outlet } from "react-router";



export default function Layout() {
const [menuopen, setMenuopen] = useState(false);

const togglemenu = () => {
setMenuopen(!menuopen);
};

return (




    <div>

<NavLink
to={"/Home"}
className="Home-container"
onClick={() => setMenuopen(false)}
>Home
</NavLink>

<NavLink
to={"/Lista"}
className="Lista-container"
onClick={() => setMenuopen(false)}
>Listan</NavLink>
    </div>


)

}

