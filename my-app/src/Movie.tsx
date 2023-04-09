import { FC, useEffect, useState } from "react";

const Movie : FC = () => {

    const [data, setData] = useState<any>([]);

    useEffect(() => {
        const getMovies = async ()=> {
            await fetch('https://localhost:3005/api/movies',{
                method: 'GET',
                headers: {
                    'Authorization': 'Bearer ' + localStorage.getItem('token'),
                    'Accept': 'application/json'
                }
            })
            .then(response => response.json())
            .then(newData => setData(newData));
        };
        getMovies();
    }, []);

    const result = data.map((item: any) => {
        return <li key={item.id}>{item.title}</li>
    });

    return <ul>
        {result}
    </ul>
}

export default Movie;