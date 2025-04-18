/* eslint-disable */
import React from 'react';
import Cookies from 'js-cookie';
import { Pagination, Dropdown, Checkbox, Form } from 'semantic-ui-react';
import ViewJobCard from '../ViewJobs/ViewJobCard.jsx';
import { BodyWrapper, loaderData } from '../../Layout/BodyWrapper.jsx';

export default class ManageJob extends React.Component {
    constructor(props) {
        super(props);
        let loader = Object.assign({}, loaderData);
        loader.allowedUsers = ["Employer", "Recruiter"];

        this.state = {
            loadJobs: [],
            loaderData: loader,
            activePage: 1,
            jobsPerPage: 6,
            sortBy: { date: "desc" },
            filter: {
                showActive: true,
                showClosed: false,
                showDraft: true,
                showExpired: false,
                showUnexpired: true,

            },
            totalPages: 1
        };

        this.loadData = this.loadData.bind(this);
        this.handleFilterChange = this.handleFilterChange.bind(this);
        this.handleSortChange = this.handleSortChange.bind(this);
        this.handlePageChange = this.handlePageChange.bind(this);
    }

    componentDidMount() {
        this.loadData();
    }

    async loadData() {

        const url = `http://localhost:51689/listing/Listing/getEmployerJobs`;
        const authToken = Cookies.get('talentAuthToken');

        if (!authToken) {
            console.error("No auth token found!");
            return;
        }

        try {
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${authToken}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);

            const data = await response.json();

            if (data.myJobs && data.myJobs.length > 0) {
                this.setState({
                    loadJobs: data.myJobs,
                    totalPages: Math.ceil(data.myJobs.length / this.state.jobsPerPage),
                    loaderData: Object.assign({}, this.state.loaderData, { isLoading: false }) // Stop loader
                });
            } else {
                console.warn("No jobs received from API.");
                this.setState({
                    loadJobs: [],
                    loaderData: Object.assign({}, this.state.loaderData, { isLoading: false }) // Stop loader
                });
            }

        } catch (error) {
            console.error("Error loading jobs:", error);
            this.setState({
                loadJobs: [],
                loaderData: Object.assign({}, this.state.loaderData, { isLoading: false }) // Stop loader
            });
        }
    }

    applyFilters(jobs) {
        const { filter } = this.state;
        return jobs.filter(job => {
            return (
                job.status !== 0 &&
                (
                    (filter.showActive && job.status === "Active") ||
                    (filter.showDraft && job.status === "Draft")
                )
            );
        });
    }

    handleFilterChange(field) {
        this.setState(prevState => {
            let updatedFilter = Object.assign({}, prevState.filter);  // Create a new copy
            updatedFilter[field] = !updatedFilter[field]; // Toggle the filter value
            return { filter: updatedFilter };
        }, this.loadData);
    }

    handleSortChange(e, { value }) {
        this.setState({ sortBy: { date: value } }, this.loadData);
    }

    handlePageChange(e, { activePage }) {
        this.setState({ activePage });
    }

    render() {
        const { loadJobs, activePage, jobsPerPage, totalPages } = this.state;

        // Pagination logic
        const indexOfLastJob = activePage * jobsPerPage;
        const indexOfFirstJob = indexOfLastJob - jobsPerPage;
        const currentJobs = loadJobs.slice(indexOfFirstJob, indexOfLastJob);

        return (
            <BodyWrapper reload={this.loadData} loaderData={this.state.loaderData}>
                <div className="ui container">
                    <h2>List Of Jobs</h2>

                    {this.state.loaderData.isLoading ? (
                        <div className="ui active loader"></div>
                    ) : (
                        <React.Fragment>
                            {/* Job Cards */}
                            <ViewJobCard jobs={currentJobs} />

                            {/* Pagination */}
                            {totalPages > 1 && (
                                <Pagination
                                    activePage={activePage}
                                    onPageChange={this.handlePageChange}
                                    totalPages={totalPages}
                                />
                            )}
                        </React.Fragment>
                    )}

                </div>
            </BodyWrapper>
        );

    }
}
