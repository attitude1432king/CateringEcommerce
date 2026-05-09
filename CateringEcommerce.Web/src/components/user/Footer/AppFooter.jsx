import { Link } from 'react-router-dom';

const companyLinks    = [
    { label: 'About Us',    to: '/about-us' },
    { label: 'Blog',        to: '/blog' },
    { label: 'Careers',     to: '/careers' },
    { label: 'Press Kit',   to: '/press-kit' },
];
const catererLinks    = [
    { label: 'Become a Partner',   to: '/become-partner' },
    { label: 'Partner Dashboard',  to: '/partner-dashboard' },
    { label: 'Partner Support',    to: '/partner-support' },
    { label: 'Growth Resources',   to: '/growth-resources' },
];
const supportLinks    = [
    { label: 'Help Center',        to: '/help-center' },
    { label: 'Contact Us',         to: '/contact-us' },
    { label: 'Terms & Conditions', to: '/terms-and-conditions' },
    { label: 'Privacy Policy',     to: '/privacy-policy' },
];

function FooterCol({ title, links }) {
    return (
        <div className="cust-footer__col">
            <h4>{title}</h4>
            <ul>
                {links.map(({ label, to }) => (
                    <li key={label}><Link to={to}>{label}</Link></li>
                ))}
            </ul>
        </div>
    );
}

export default function AppFooter() {
    const year = new Date().getFullYear();

    return (
        <footer className="cust-footer" role="contentinfo" aria-label="Footer">
            <div className="cust-footer__top">
                {/* Brand */}
                <div className="cust-footer__brand">
                    <img src="/logo-white.svg" alt="ENYVORA" />
                    <p>
                        Your premium event catering platform connecting exceptional caterers with
                        memorable celebrations. Book with confidence, celebrate with style.
                    </p>
                    {/* Socials */}
                    <div className="flex items-center gap-3 mt-5">
                        {[
                            {
                                label: 'Facebook', href: 'https://facebook.com',
                                path: 'M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z',
                            },
                            {
                                label: 'Instagram', href: 'https://instagram.com',
                                path: 'M12.315 2c2.43 0 2.784.013 3.808.06 1.064.049 1.791.218 2.427.465a4.902 4.902 0 011.772 1.153 4.902 4.902 0 011.153 1.772c.247.636.416 1.363.465 2.427.048 1.067.06 1.407.06 4.123v.08c0 2.643-.012 2.987-.06 4.043-.049 1.064-.218 1.791-.465 2.427a4.902 4.902 0 01-1.153 1.772 4.902 4.902 0 01-1.772 1.153c-.636.247-1.363.416-2.427.465-1.067.048-1.407.06-4.123.06h-.08c-2.643 0-2.987-.012-4.043-.06-1.064-.049-1.791-.218-2.427-.465a4.902 4.902 0 01-1.772-1.153 4.902 4.902 0 01-1.153-1.772c-.247-.636-.416-1.363-.465-2.427-.047-1.024-.06-1.379-.06-3.808v-.63c0-2.43.013-2.784.06-3.808.049-1.064.218-1.791.465-2.427a4.902 4.902 0 011.153-1.772A4.902 4.902 0 015.45 2.525c.636-.247 1.363-.416 2.427-.465C8.901 2.013 9.256 2 11.685 2h.63zm-.081 1.802h-.468c-2.456 0-2.784.011-3.807.058-.975.045-1.504.207-1.857.344-.467.182-.8.398-1.15.748-.35.35-.566.683-.748 1.15-.137.353-.3.882-.344 1.857-.047 1.023-.058 1.351-.058 3.807v.468c0 2.456.011 2.784.058 3.807.045.975.207 1.504.344 1.857.182.466.399.8.748 1.15.35.35.683.566 1.15.748.353.137.882.3 1.857.344 1.054.048 1.37.058 4.041.058h.08c2.597 0 2.917-.01 3.96-.058.976-.045 1.505-.207 1.858-.344.466-.182.8-.398 1.15-.748.35-.35.566-.683.748-1.15.137-.353.3-.882.344-1.857.048-1.055.058-1.37.058-4.041v-.08c0-2.597-.01-2.917-.058-3.96-.045-.976-.207-1.505-.344-1.858a3.097 3.097 0 00-.748-1.15 3.098 3.098 0 00-1.15-.748c-.353-.137-.882-.3-1.857-.344-1.023-.047-1.351-.058-3.807-.058zM12 6.865a5.135 5.135 0 110 10.27 5.135 5.135 0 010-10.27zm0 1.802a3.333 3.333 0 100 6.666 3.333 3.333 0 000-6.666zm5.338-3.205a1.2 1.2 0 110 2.4 1.2 1.2 0 010-2.4z',
                            },
                            {
                                label: 'Twitter', href: 'https://twitter.com',
                                path: 'M8.29 20c7.547 0 11.675-6.253 11.675-11.675 0-.178 0-.355-.012-.53A8.348 8.348 0 0022 5.92a8.19 8.19 0 01-2.357.646 4.118 4.118 0 001.804-2.27 8.224 8.224 0 01-2.605.996 4.107 4.107 0 00-6.993 3.743 11.65 11.65 0 01-8.457-4.287 4.106 4.106 0 001.27 5.477A4.072 4.072 0 012.8 9.713v.052a4.105 4.105 0 003.292 4.022 4.095 4.095 0 01-1.853.07 4.108 4.108 0 003.834 2.85A8.233 8.233 0 012 18.407a11.616 11.616 0 006.29 1.84',
                            },
                        ].map(({ label, href, path }) => (
                            <a
                                key={label}
                                href={href}
                                target="_blank"
                                rel="noopener noreferrer"
                                aria-label={label}
                                className="inline-flex items-center justify-center w-9 h-9 rounded-full bg-white/10 hover:bg-primary transition-colors duration-200"
                            >
                                <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
                                    <path d={path} />
                                </svg>
                            </a>
                        ))}
                    </div>
                </div>

                <FooterCol title="Company"     links={companyLinks} />
                <FooterCol title="For Caterers" links={catererLinks} />
                <FooterCol title="Support"      links={supportLinks} />

                {/* App / contact column */}
                <div className="cust-footer__col">
                    <h4>Get the App</h4>
                    <ul>
                        <li><a href="#">iOS App Store</a></li>
                        <li><a href="#">Google Play</a></li>
                    </ul>
                    <h4 className="mt-6">Contact</h4>
                    <ul>
                        <li><a href="mailto:hello@enyvora.com">hello@enyvora.com</a></li>
                        <li><a href="tel:+918000000000">+91 80000 00000</a></li>
                    </ul>
                </div>
            </div>

            {/* Bottom bar */}
            <div className="cust-footer__bottom">
                <span>© {year} ENYVORA. All rights reserved.</span>
                <div className="cust-footer__pay">
                    <span>RAZORPAY</span>
                    <span>UPI</span>
                    <span>VISA</span>
                    <span>MASTERCARD</span>
                    <span>NETBANKING</span>
                </div>
                <div className="flex items-center gap-5">
                    <a href="#" className="hover:text-neutral-300 transition-colors">Sitemap</a>
                    <a href="#" className="hover:text-neutral-300 transition-colors">Accessibility</a>
                    <a href="#" className="hover:text-neutral-300 transition-colors">Cookies</a>
                </div>
            </div>
        </footer>
    );
}
