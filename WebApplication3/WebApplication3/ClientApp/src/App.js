import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { SignInPage } from './components/SignInPage';
import { RegisterUser } from './components/RegisterUser';
import { AccountPage } from './components/AccountPage';

import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
            <Route exact path='/' component={SignInPage} />
            <Route path='/account/new' component={RegisterUser} />
            <Route path='/account/view/:id' component={AccountPage} />
      </Layout>
    );
  }
}
