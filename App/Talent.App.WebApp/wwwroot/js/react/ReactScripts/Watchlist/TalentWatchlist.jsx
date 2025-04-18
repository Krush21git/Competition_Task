import React from 'react';
import Cookies from 'js-cookie';
import { BodyWrapper, loaderData } from '../Layout/BodyWrapper.jsx';
import TalentCard from '../TalentFeed/TalentCard.jsx';

export default class TalentWatchlist extends React.Component {
    constructor(props) {
        super(props);

        let loader = Object.assign({}, loaderData); // Fix: Avoid spread operator
        loader.allowedUsers.push("Employer", "Recruiter");

        this.state = {
            loaderData: loader,
            watchlist: [] // Stores fetched watchlist data
        };
    }

    componentDidMount() {
        this.loadWatchlist();
    }

    /** Standard class method for fetching watchlist */
    loadWatchlist() {
        const url = 'http://localhost:51689/listing/listing/getWatchlist';
        const authToken = Cookies.get('talentAuthToken');

        if (!authToken) {
            console.error("No auth token found. User may not be authenticated.");
            return;
        }

        fetch(url, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${authToken}`,
                'Content-Type': 'application/json'
            }
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to fetch watchlist');
                }
                return response.json();
            })
            .then(data => {
                console.log("Watchlist Data:", data);
                this.setState({ watchlist: data });
            })
            .catch(error => {
                console.error("Error fetching watchlist:", error);
            });
    }

    render() {
        const { watchlist, loaderData } = this.state;

        return (
            <BodyWrapper reload={this.loadWatchlist.bind(this)} loaderData={loaderData}>
                <div className="watchlist-container" style={{ padding: '20px' }}>
                    {watchlist.length > 0 ? (
                        watchlist.map((talent, index) => (
                            <TalentCard key={talent.id || index} talentData={talent} />
                        ))
                    ) : (
                        <p>No talents in the watchlist.</p>
                    )}
                </div>
            </BodyWrapper>
        );
    }
}
