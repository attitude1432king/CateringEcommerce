

function AppFooter() {
    return (
        <footer className="bg-neutral-800 text-neutral-300 py-12">
            <div className="container mx-auto px-4 sm:px-6 lg:px-8">
                <div className="grid grid-cols-1 md:grid-cols-4 gap-8 mb-8">
                    <div>
                        <h5 className="text-lg font-semibold text-white mb-3 flex items-center"><span className="icon-placeholder text-xl mr-1">🍽️</span>QuickFeast</h5>
                        <p className="text-sm">Your one-stop solution for event catering. Delicious food, delivered.</p>
                    </div>
                    <div>
                        <h5 className="text-md font-semibold text-white mb-3">Company</h5>
                        <ul className="space-y-2 text-sm">
                            <li><a href="#" className="hover:text-rose-400">About Us</a></li>
                            <li><a href="#" className="hover:text-rose-400">Careers</a></li>
                            <li><a href="#" className="hover:text-rose-400">Contact Us</a></li>
                        </ul>
                    </div>
                    <div>
                        <h5 className="text-md font-semibold text-white mb-3">For Caterers</h5>
                        <ul className="space-y-2 text-sm">
                            <li><a href="#" className="hover:text-rose-400">Partner with Us</a></li>
                            <li><a href="#" className="hover:text-rose-400">Caterer Login</a></li>
                        </ul>
                    </div>
                    <div>
                        <h5 className="text-md font-semibold text-white mb-3">Legal</h5>
                        <ul className="space-y-2 text-sm">
                            <li><a href="#" className="hover:text-rose-400">Terms & Conditions</a></li>
                            <li><a href="#" className="hover:text-rose-400">Privacy Policy</a></li>
                            <li><a href="#" className="hover:text-rose-400">FAQ</a></li>
                        </ul>
                    </div>
                </div>
                <div className="border-t border-neutral-700 pt-8 flex flex-col sm:flex-row justify-between items-center text-sm">
                    <p>&copy; {new Date().getFullYear()} QuickFeast. All rights reserved.</p>
                    <div className="flex space-x-4 mt-4 sm:mt-0">
                        {/* Placeholder for social media icons */}
                        <a href="#" className="hover:text-rose-400">Facebook</a>
                        <a href="#" className="hover:text-rose-400">Instagram</a>
                        <a href="#" className="hover:text-rose-400">Twitter</a>
                    </div>
                </div>
            </div>
        </footer>
    );
}

