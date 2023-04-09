import React from 'react';

const Layout = ({ children } : any) => {
  return (
    <div>
      <header>
        {/* Add your header content here */}
      </header>
      <main>
        {children}
      </main>
      <footer>
        {/* Add your footer content here */}
      </footer>
    </div>
  );
};

export default Layout;