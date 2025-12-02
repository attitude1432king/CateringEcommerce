/*
========================================
File: src/pages/CateringDetailPage.jsx (NEW FILE)
========================================
The immersive detail page with animations and detailed sections.
*/
import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion'; // Need to install framer-motion
import Loader from '../components/common/Loader';

// --- Mock Data for Detail Page ---
const mockDetails = {
    id: 1,
    name: "The Royal Feast",
    rating: 4.5,
    ratingCount: "1K+",
    location: "Adajan, Surat",
    coverImages: [
        "https://placehold.co/1200x400/ffedd5/9a3412?text=Delicious+Buffet+Setup",
        "https://placehold.co/1200x400/ffe4e6/be123c?text=Fresh+Ingredients",
        "https://placehold.co/1200x400/dbeafe/1e40af?text=Professional+Staff"
    ],
    packages: [
        { id: 101, name: "Silver Veg Package", price: 350, items: ["Paneer Butter Masala", "Dal Fry", "Jeera Rice", "2 Rotis", "Salad"] },
        { id: 102, name: "Gold Premium Package", price: 550, items: ["Welcome Drink", "2 Starters", "Paneer Lababdar", "Veg Kofta", "Dal Makhani", "Veg Biryani", "Naan/Roti", "Gulab Jamun"] }
    ],
    foodItems: {
        "Starters": [
            { id: 201, name: "Hara Bhara Kabab", price: 150, image: "https://placehold.co/150x150?text=Kabab" },
            { id: 202, name: "Spring Rolls", price: 120, image: "https://placehold.co/150x150?text=Rolls" }
        ],
        "Main Course": [
            { id: 203, name: "Paneer Tikka Masala", price: 280, image: "https://placehold.co/150x150?text=Paneer" },
            { id: 204, name: "Malai Kofta", price: 260, image: "https://placehold.co/150x150?text=Kofta" }
        ]
    },
    decorations: [
        { id: 301, name: "Royal Wedding Theme", image: "https://placehold.co/400x300/fef3c7/78350f?text=Royal+Decor" },
        { id: 302, name: "Minimalist Floral", image: "https://placehold.co/400x300/dcfce7/14532d?text=Floral+Decor" }
    ],
    kitchenMedia: [
        { id: 401, type: 'image', src: "https://placehold.co/400x300?text=Clean+Kitchen" },
        { id: 402, type: 'video', src: "https://www.w3schools.com/html/mov_bbb.mp4" } // Sample video
    ]
};


export default function CateringDetailPage() {
    const { id } = useParams();
    const navigate = useNavigate();
    const [details, setDetails] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [activeSection, setActiveSection] = useState('packages');

    useEffect(() => {
        // Simulate API fetch
        setTimeout(() => {
            setDetails(mockDetails);
            setIsLoading(false);
        }, 500);

        // Lock body scroll when overlay is open
        document.body.style.overflow = 'hidden';
        return () => {
            document.body.style.overflow = 'unset';
        };
    }, [id]);

    const handleBack = () => {
        navigate(-1); // Go back to list
    };

    if (isLoading) return <div className="fixed inset-0 z-50 bg-white flex justify-center items-center"><Loader /></div>;
    if (!details) return null;

    return (
        <motion.div
            initial={{ y: "100%" }}
            animate={{ y: 0 }}
            exit={{ y: "100%" }}
            transition={{ type: "spring", damping: 25, stiffness: 200 }}
            className="fixed inset-0 z-[100] bg-white overflow-y-auto"
        >
            {/* Sticky Header */}
            <div className="sticky top-0 z-10 bg-white shadow-sm border-b border-neutral-100 px-4 py-3 flex items-center justify-between">
                <div className="flex items-center gap-4">
                    <button onClick={handleBack} className="p-2 hover:bg-neutral-100 rounded-full transition-colors">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-neutral-700" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" /></svg>
                    </button>
                    <div>
                        <h1 className="text-lg font-bold text-neutral-900 leading-tight">{details.name}</h1>
                        <div className="flex items-center text-xs text-neutral-500">
                            <span className="bg-green-100 text-green-800 px-1 rounded font-bold mr-1">{details.rating} ★</span>
                            <span>({details.ratingCount} ratings) • {details.location}</span>
                        </div>
                    </div>
                </div>
                <button className="p-2 hover:bg-neutral-100 rounded-full text-neutral-600">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.368 2.684 3 3 0 00-5.368-2.684z" /></svg>
                </button>
            </div>

            {/* Hero Banner Slider */}
            <div className="relative h-64 md:h-80 lg:h-96 bg-neutral-200 overflow-hidden">
                <img src={details.coverImages[0]} alt="Cover" className="w-full h-full object-cover" />
                <div className="absolute bottom-4 right-4 bg-black bg-opacity-60 text-white text-xs px-2 py-1 rounded">1/3 Photos</div>
            </div>

            {/* Tab Navigation */}
            <div className="sticky top-[60px] z-10 bg-white border-b border-neutral-200 px-4">
                <div className="flex space-x-6 overflow-x-auto no-scrollbar">
                    {['Packages', 'Food Items', 'Decorations', 'Kitchen View'].map((tab) => {
                        const id = tab.toLowerCase().replace(' ', '');
                        return (
                            <button
                                key={tab}
                                onClick={() => {
                                    setActiveSection(id);
                                    document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
                                }}
                                className={`py-3 whitespace-nowrap text-sm font-medium border-b-2 transition-colors ${activeSection === id ? 'border-rose-600 text-rose-600' : 'border-transparent text-neutral-500 hover:text-neutral-800'
                                    }`}
                            >
                                {tab}
                            </button>
                        )
                    })}
                </div>
            </div>

            <div className="max-w-5xl mx-auto p-4 space-y-12 pb-20">

                {/* 1. Packages Section */}
                <section id="packages" className="scroll-mt-32">
                    <h2 className="text-xl font-bold text-neutral-800 mb-4">Popular Packages</h2>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {details.packages.map(pkg => (
                            <div key={pkg.id} className="border border-neutral-200 rounded-xl p-4 shadow-sm hover:shadow-md transition-shadow">
                                <div className="flex justify-between items-start">
                                    <h3 className="font-bold text-lg text-neutral-800">{pkg.name}</h3>
                                    <span className="bg-rose-50 text-rose-700 px-2 py-1 rounded text-sm font-bold">₹{pkg.price}/plate</span>
                                </div>
                                <p className="text-xs text-neutral-500 mt-1">{pkg.items.length} items included</p>
                                <ul className="mt-3 space-y-1">
                                    {pkg.items.slice(0, 4).map((item, i) => (
                                        <li key={i} className="text-sm text-neutral-600 flex items-center gap-2">
                                            <span className="w-1 h-1 bg-neutral-400 rounded-full"></span> {item}
                                        </li>
                                    ))}
                                    {pkg.items.length > 4 && <li className="text-xs text-rose-600 font-medium pl-3">+{pkg.items.length - 4} more items</li>}
                                </ul>
                                <button className="w-full mt-4 border border-rose-200 text-rose-600 font-semibold py-2 rounded-lg hover:bg-rose-50 transition-colors">View Details</button>
                            </div>
                        ))}
                    </div>
                </section>

                {/* 2. Food Items Section */}
                <section id="fooditems" className="scroll-mt-32">
                    <h2 className="text-xl font-bold text-neutral-800 mb-4">A La Carte Menu</h2>
                    {Object.entries(details.foodItems).map(([category, items]) => (
                        <div key={category} className="mb-6">
                            <h3 className="text-lg font-semibold text-neutral-700 mb-3 border-l-4 border-rose-500 pl-3">{category}</h3>
                            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-2 gap-4">
                                {items.map(item => (
                                    <div key={item.id} className="flex gap-3 p-3 border border-neutral-100 rounded-lg shadow-sm bg-white">
                                        <div className="w-24 h-24 flex-shrink-0 rounded-lg overflow-hidden bg-neutral-100">
                                            <img src={item.image} alt={item.name} className="w-full h-full object-cover" />
                                        </div>
                                        <div className="flex-1 flex flex-col justify-between">
                                            <div>
                                                <h4 className="font-semibold text-neutral-800">{item.name}</h4>
                                                <p className="text-sm font-bold text-neutral-600">₹{item.price}</p>
                                            </div>
                                            <button className="self-end text-xs font-bold bg-white text-green-600 border border-green-600 px-4 py-1 rounded shadow-sm hover:bg-green-50 uppercase">
                                                Add +
                                            </button>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    ))}
                </section>

                {/* 3. Decorations Section */}
                <section id="decorations" className="scroll-mt-32">
                    <h2 className="text-xl font-bold text-neutral-800 mb-4">Decoration Themes</h2>
                    <div className="flex gap-4 overflow-x-auto pb-4 no-scrollbar">
                        {details.decorations.map(decor => (
                            <div key={decor.id} className="min-w-[250px] rounded-xl overflow-hidden border border-neutral-200 shadow-sm">
                                <div className="h-40 bg-neutral-100">
                                    <img src={decor.image} alt={decor.name} className="w-full h-full object-cover" />
                                </div>
                                <div className="p-3">
                                    <h4 className="font-semibold text-neutral-800">{decor.name}</h4>
                                </div>
                            </div>
                        ))}
                    </div>
                </section>

                {/* 4. Kitchen View Section */}
                <section id="kitchenview" className="scroll-mt-32">
                    <h2 className="text-xl font-bold text-neutral-800 mb-4">Kitchen & Hygiene</h2>
                    <div className="grid grid-cols-2 md:grid-cols-3 gap-2">
                        {details.kitchenMedia.map(media => (
                            <div key={media.id} className="aspect-square rounded-lg overflow-hidden bg-neutral-100">
                                {media.type === 'image' ? (
                                    <img src={media.src} className="w-full h-full object-cover" />
                                ) : (
                                    <video src={media.src} className="w-full h-full object-cover" controls />
                                )}
                            </div>
                        ))}
                    </div>
                </section>

            </div>
        </motion.div>
    );
}