import React, { useState, useEffect } from 'react'
import { Container } from 'reactstrap'
import { NavMenu } from './NavMenu'
import { getSession } from '../utils/Sessions'

const Layout = ({children}) => {
  const [loggedIn, setLoggedIn] = useState(getSession() !== null)
  const [user, setUser] = useState(getSession())

  useEffect(() => {
    window.onstorage = () => {
      const session = getSession()
      setLoggedIn(session !== null)
      setUser(session)
    }
  },[])

  return (
    <div>
      <NavMenu loggedIn={loggedIn} user={user} />
      <Container>
        {children}
      </Container>
    </div>
  )
}

export default Layout