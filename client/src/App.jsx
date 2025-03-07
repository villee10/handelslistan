
import Home from './Home.jsx'
import Lista from './Lista.jsx'
import Layout from './layout.jsx'
import './App.css'
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'

function App() {
return (
<Router>
<Routes>
            {/* Första sidan*/}
  <Route path="/" element={<Home /> } />
  <Route path="/Lista" element={<Lista /> } />
  
</Routes>
</Router>



);
}

export default App
