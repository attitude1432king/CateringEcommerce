import { useDefaultCity } from '../../hooks/useDefaultCity';

const CITY_LIST = [];

//const CITY_LIST = [
//    'Surat',
//    'Ahmedabad',
//    'Mumbai',
//    'Delhi',
//    'Bengaluru'
//];

export default function CitySelector() {
    const { city, loading, setCity } = useDefaultCity();

    if (loading) {
        return <span>Detecting city...</span>;
    }

    return (
        <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
            <label>City:</label>

            <select
                value={city || ''}
                onChange={(e) => setCity(e.target.value)}
            >
                {CITY_LIST.map((c) => (
                    <option key={c} value={c}>
                        {c}
                    </option>
                ))}
            </select>
        </div>
    );
}
