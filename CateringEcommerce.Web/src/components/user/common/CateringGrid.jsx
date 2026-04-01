// File: src/components/user/CatererGrid.jsx
import React, { useEffect, useState } from 'react';
import CatererCard from './CateringCard';
import { locationApiService } from '../../../services/locationApi';
import { CITY_KEY } from '../../../utils/cityStorage';


const defaultData = [
    { id: 1, name: 'Royal Banquet Caterers', coverImage: 'https://placehold.co/800x600', cuisines: ['Indian', 'Gujarati'], rating: 4.3, location: 'Ashram Road', distance: 1.9, priceRange: '₹350-₹600', isOpen: true, offer: 'Flat 10% OFF' },
    { id: 2, name: 'Green Feast Caterers', coverImage: 'https://placehold.co/800x600', cuisines: ['Continental', 'Italian'], rating: 4.2, location: 'Thaltej', distance: 5.1, priceRange: '₹200-₹400', isOpen: true, offer: 'Flat 15% OFF' },
    { id: 3, name: 'Banquet Pro', coverImage: 'https://placehold.co/800x600', cuisines: ['Party Platters', 'BBQ'], rating: 4.0, location: 'Maninagar', distance: 3.2, priceRange: '₹150-₹300', isOpen: false, offer: 'Free Decor Addon' },
    { id: 4, name: 'Gourmet Gatherings', coverImage: 'https://placehold.co/800x600', cuisines: ['Italian', 'Continental'], rating: 4.8, location: 'Vastrapur', distance: 6.2, priceRange: '₹500-₹900', isOpen: true },
    { id: 5, name: 'Spice Route Catering', coverImage: 'https://placehold.co/800x600', cuisines: ['Indian', 'Asian'], rating: 4.9, location: 'Navrangpura', distance: 2.8, priceRange: '₹300-₹700', isOpen: true }
];


//export default function CatererGrid({ city = defaultData }) {
export default function CatererGrid() {
    const [data, setData] = useState([]);

    const fetchCateringList = async (cityName) => {
        try {
            const response = await locationApiService.getVerifiedCateringListAsync(cityName);
            if (response.result) {
                setData(response.data);
            }
        } catch (error) {
            console.error("Failed to load Catering List: " + error);
        }
    }

    useEffect(() => {
        var cityName = localStorage[CITY_KEY] ?? ''
        if (localStorage[CITY_KEY]) {
            fetchCateringList(cityName);
        }
    }, []);


    return (
        <section
            id="caterers"
            className="py-8 px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto"
        >
            <div className="flex items-center justify-between mb-6">
                <h2 className="text-2xl font-semibold">Featured Caterers</h2>
                <a href="#" className="text-sm text-rose-600 hover:underline">
                    View all
                </a>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                {data.map(c => (
                    <CatererCard catering={c} key={c.id} />
                ))}
            </div>
        </section>
    );
}
