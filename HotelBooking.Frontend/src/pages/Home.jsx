import React from 'react';
import SearchForm from '../components/SearchForm';

const Home = () => {
  return (
    <div>
      <div className="hero">
        <h1>Find Your Perfect Stay</h1>
        <p>Search and book hotels worldwide with ease</p>
      </div>

      <SearchForm />
    </div>
  );
};

export default Home;
