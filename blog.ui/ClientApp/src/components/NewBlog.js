import React from 'react'
import Auth from './Auth'

const NewBlog = () => {
    return (
        <h1>New Blog!</h1>
    )
}

export default Auth(NewBlog, {roles: ['admin']})