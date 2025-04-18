import React from 'react';
import PropTypes from 'prop-types';
import { withRouter } from 'react-router-dom';
import Cookies from 'js-cookie';
import {
    Card,
    CardContent,
    CardHeader,
    CardMeta,
    CardDescription,
    Button,
    Label,
    CardGroup,
    Icon
} from 'semantic-ui-react';

class ViewJobCard extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            currentPage: 1,
            jobsPerPage: 6,
            loadJobs: props.jobs || [],
            message: ''
        };

        this.closeJob = this.closeJob.bind(this);
    }

    closeJob(jobId) {
        var self = this;
        var link = "http://localhost:51689/listing/listing/closeJob?id=" + jobId;
        var cookies = Cookies.get('talentAuthToken');

        if (window.confirm("Are you sure you want to close this job?")) {
            fetch(link, {
                method: 'POST',
                headers: {
                    'Authorization': 'Bearer ' + cookies,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(jobId)
            })
                .then(function (response) { return response.json(); })
                .then(function (res) {
                    if (res.success) {
                        var updatedJobs = self.state.loadJobs.map(function (job) {
                            if (job.id === jobId) {
                                job.status = 1;
                            }
                            return job;
                        });

                        self.setState({
                            loadJobs: updatedJobs,
                            message: "Job successfully closed!"
                        });

                        alert("Job successfully closed!");
                    }
                });
        }
    }

    render() {
        var loadJobs = this.state.loadJobs;
        var currentPage = this.state.currentPage;
        var jobsPerPage = this.state.jobsPerPage;

        var indexOfLastJob = currentPage * jobsPerPage;
        var indexOfFirstJob = indexOfLastJob - jobsPerPage;
        var currentJobs = loadJobs.slice(indexOfFirstJob, indexOfLastJob);

        return (
            <div>
                <CardGroup style={{ display: "flex", flexWrap: "wrap", gap: "20px" }}>
                    {
                        currentJobs.length > 0 ? currentJobs.map(function (job) {
                            return (
                                <Card key={job.id} color={job.expired ? 'red' : 'green'} style={{
                                    flex: "1 1 300px",
                                    minWidth: "300px",
                                    maxWidth: "350px",
                                    minHeight: "400px",
                                    display: "flex",
                                    flexDirection: "column",
                                    justifyContent: "space-between"
                                }}>
                                    <CardContent>
                                        <CardHeader>{job.title || "Unknown Job"}</CardHeader>
                                        <Label attached="top right" color="black" style={{ borderRadius: "5px", display: "flex", alignItems: "center", padding: "6px 10px" }}>
                                            <Icon name="user" style={{ marginRight: "5px" }} />
                                            {job.applicantCount || 0}
                                        </Label>
                                        <CardMeta>
                                            {(job.location && job.location.city ? job.location.city : "No Location") + ", " +
                                                (job.location && job.location.country ? job.location.country : "")}
                                        </CardMeta>
                                        <CardDescription>
                                            {job.summary || "No description available."}
                                        </CardDescription>
                                    </CardContent>
                                    <CardContent extra>
                                        <div style={{
                                            display: "flex",
                                            alignItems: "center",
                                            gap: "8px",
                                            flexWrap: "nowrap",
                                            justifyContent: "space-between",
                                            width: "100%",
                                            overflow: "hidden"
                                        }}>
                                            {
                                                job.status === 1 ?
                                                    <Label color="grey">Closed Job</Label> :
                                                    <Label color="red">Expired</Label>
                                            }
                                            {job.status !== 1 && (
                                                <div>
                                                    <Button
                                                        style={{ flex: "1", minWidth: "70px", padding: "6px 10px" }}
                                                        basic
                                                        color="blue"
                                                        icon="close"
                                                        content="Close"
                                                        onClick={this.closeJob.bind(this, job.id)}
                                                        disabled={job.status === 1}
                                                    />

                                                    <Button
                                                        style={{ flex: "1", minWidth: "70px", padding: "6px 10px" }}
                                                        basic
                                                        color="blue"
                                                        icon="edit"
                                                        content="Edit"
                                                        onClick={() => this.props.history.push(`/create-job/${job.id}`)}
                                                    />

                                                    <Button
                                                        style={{ flex: "1", minWidth: "70px", padding: "6px 10px" }}
                                                        basic
                                                        color="blue"
                                                        icon="copy"
                                                        content="Copy"
                                                    />

                                                </div>
                                            )}
                                        </div>
                                    </CardContent>
                                </Card>
                            );
                        }.bind(this)) : (
                            <p>No jobs found.</p>
                        )
                    }
                </CardGroup>
            </div>
        );
    }
}

ViewJobCard.propTypes = {
    jobs: PropTypes.arrayOf(PropTypes.shape({
        id: PropTypes.string.isRequired,
        title: PropTypes.string,
        expired: PropTypes.bool,
        summary: PropTypes.string,
        location: PropTypes.shape({
            city: PropTypes.string,
            country: PropTypes.string
        }),
        applicantCount: PropTypes.number,
        status: PropTypes.number
    })),
    history: PropTypes.object.isRequired
};

export default withRouter(ViewJobCard);
