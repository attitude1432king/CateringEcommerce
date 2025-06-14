

// Components (AppHeader, HeroSection, etc. - mostly unchanged, AppHeader updated)
function AppHeader({ isAuthenticated, onAuthToggle, onOpenAuthModal }) { // Added onOpenAuthModal
    return (
        <header className="bg-white shadow-md sticky top-0 z-50">
            <div className="container mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex items-center justify-between h-16">
                    <div className="flex items-center">
                        <div className="flex-shrink-0">
                            <a href="#" className="text-3xl font-bold text-rose-600 flex items-center">
                                <span className="icon-placeholder text-3xl mr-1">🍽️</span> QuickFeast
                            </a>
                        </div>
                    </div>
                    <div className="hidden md:flex items-center space-x-4 flex-grow justify-center">
                        <div className="relative">
                            <input
                                type="text"
                                placeholder="Enter your location"
                                className="pl-10 pr-4 py-2 border border-neutral-300 rounded-md focus:ring-rose-500 focus:border-rose-500 sm:text-sm"
                            />
                            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                <svg className="h-5 w-5 text-neutral-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clipRule="evenodd" />
                                </svg>
                            </div>
                        </div>
                        <div className="relative flex-grow max-w-xs lg:max-w-md">
                            <input
                                type="search"
                                placeholder="Search for caterers, cuisines..."
                                className="w-full pl-10 pr-4 py-2 border border-neutral-300 rounded-md focus:ring-rose-500 focus:border-rose-500 sm:text-sm"
                            />
                            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                <svg className="h-5 w-5 text-neutral-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clipRule="evenodd" />
                                </svg>
                            </div>
                        </div>
                        <a href="#" className="text-neutral-600 hover:text-rose-600 px-3 py-2 rounded-md text-sm font-medium">Offers</a>
                    </div>
                    <div className="flex items-center space-x-3">
                        {isAuthenticated ? (
                            <div className="relative group">
                                <button className="flex items-center text-sm font-medium text-neutral-600 hover:text-rose-600">
                                    Hi, User
                                    <svg className="ml-1 h-4 w-4" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clipRule="evenodd" /></svg>
                                </button>
                                <div className="absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg py-1 z-20 hidden group-hover:block">
                                    <a href="#" className="block px-4 py-2 text-sm text-neutral-700 hover:bg-amber-100 hover:text-rose-600">My Profile</a>
                                    <a href="#" className="block px-4 py-2 text-sm text-neutral-700 hover:bg-amber-100 hover:text-rose-600">My Orders</a>
                                    <button onClick={onAuthToggle} className="block w-full text-left px-4 py-2 text-sm text-neutral-700 hover:bg-amber-100 hover:text-rose-600">Logout</button>
                                </div>
                            </div>
                        ) : (
                            <button onClick={onOpenAuthModal} className="bg-rose-600 text-white px-4 py-2 rounded-md text-sm font-medium hover:bg-rose-700">
                                Login / Sign Up
                            </button>
                        )}
                        <a href="#" className="text-neutral-600 hover:text-rose-600 p-2 relative">
                            <svg className="h-6 w-6" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
                            </svg>
                            <span className="absolute top-0 right-0 inline-flex items-center justify-center px-2 py-1 text-xs font-bold leading-none text-red-100 transform translate-x-1/2 -translate-y-1/2 bg-red-600 rounded-full">3</span>
                        </a>
                        <button className="md:hidden text-neutral-600 hover:text-rose-600">
                            <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M4 6h16M4 12h16M4 18h16"></path></svg>
                        </button>
                    </div>
                </div>
            </div>
        </header>
    );
}

