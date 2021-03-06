import React, { Component } from 'react';
import * as getTeachListActions from './reducer';
import * as getTeachSubjListActions from './reducer';
import * as changeTeachSubjListActions from './reducer';
import { connect } from 'react-redux';
import get from "lodash.get";
import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import ListItemText from '@material-ui/core/ListItemText';
import Grid from '@material-ui/core/Grid';
import FormLabel from '@material-ui/core/FormLabel';
import FormControl from '@material-ui/core/FormControl';
import FormGroup from '@material-ui/core/FormGroup';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import Checkbox from '@material-ui/core/Checkbox';
import Paper from '@material-ui/core/Paper';
import { Growl } from 'primereact/growl';
import { MDBBtn } from "mdbreact";
import ListSubheader from '@material-ui/core/ListSubheader';

import './SetTeachSubjStyles.css';

class SetTeacherSubjects extends Component {
    state = {
        selectedSubjects: [],
        selectedTeacherId: '',
        isSmtChanged:false
    };
    componentWillReceiveProps = (nextProps) => {
        if (nextProps !== this.props) {
            if (this.state.selectedSubjects !== nextProps.teacherSubjects) {
                console.log("np: ", nextProps);
                this.setState({ selectedSubjects: nextProps.teacherSubjects });
            }
            if (nextProps.success === true && nextProps.failed === false && typeof nextProps.messageResult != 'object') {
                this.growl.show({ life: 6000, severity: 'success', summary: 'Success', detail: nextProps.messageResult });
            } else if (nextProps.success === false && nextProps.failed === true) {
                this.growl.show({ life: 6000, severity: 'error', summary: 'Error', detail: nextProps.errors });
            }
        }
    }
    componentDidMount = () => {
        this.props.getTeachers();
    }
    changeTeacherOnClick = (value) => () => {
        this.setState({ selectedTeacherId: value });
        this.props.getTeacherSubjects({ teacherId: value });
        //const { teacherSubjects } = this.props;
        //console.log(teacherSubjects);
    }
    changeSubjectsOnChanged = (event) => {
        const { selectedSubjects } = this.state;
        let objIndex = selectedSubjects.findIndex((item => item.subjectName === event.target.name));
        if (event.target.checked === true) {
            selectedSubjects[objIndex].isActive = true;
        }
        else {
            selectedSubjects[objIndex].isActive = false;
        }
        this.setState({ selectedSubjects: selectedSubjects });
    }
    saveChangesOnClick = () => {
        const { selectedSubjects, selectedTeacherId } = this.state;
        let where = selectedSubjects.filter(item => item.isActive === true);
        if (where.length > 0) {
            let selected = where.map(item => {
                return item.subjectName;
            });
            this.props.changeTeacherSubjects({ teacherId: selectedTeacherId, subjects: selected });
        }
    }
    render() {
        const { teachers, teacherSubjects } = this.props;
        console.log("RENDER", teachers, teacherSubjects);
        let counter = 0;

        return (
            <Grid container justify="space-between">
                <Paper className="listW mt-3" elevation={7}>
                    <List component="nav" aria-label="secondary mailbox folder"
                    subheader={
                        <ListSubheader className="subhead" component="div">
                          Список вчителів
                        </ListSubheader>
                      }
                    >
                        {
                            teachers.map(item => {
                                return (
                                    <ListItem
                                        key={item.id}
                                        button
                                        onClick={this.changeTeacherOnClick(item.id)}
                                        selected={this.state.selectedTeacherId === item.id}
                                    >
                                        <ListItemText primary={item.name} />
                                    </ListItem>
                                )
                            })
                        }
                    </List>
                </Paper>
                <Paper hidden={this.state.selectedSubjects.length===0}  elevation={10} className="mt-3 pl-4" >
                    <FormControl className="formW mt-2" component="fieldset" >
                        <FormLabel className="legend" component="legend">Змінити спеціалізацію</FormLabel>
                        <FormGroup>
                            {
                                teacherSubjects.map(item => {
                                    return (
                                        <FormControlLabel
                                            control={<Checkbox checked={this.state.selectedSubjects[counter].isActive} onChange={this.changeSubjectsOnChanged} name={item.subjectName} />}
                                            key={counter++}
                                            label={item.subjectName}
                                        />
                                    )
                                })
                            }
                        </FormGroup>
                    </FormControl>
                </Paper>
                <Grid container direction="column" alignItems="flex-end" hidden={this.state.selectedTeacherId === ''}>
                    <Growl className="mt-5" ref={(el) => this.growl = el} />
                    <MDBBtn className="btnSave" onClick={this.saveChangesOnClick} className="mt-4 mb-4 purple-gradient border-0 px-5 py-2" >
                        Save
                    </MDBBtn>
                </Grid>
            </Grid>
        );
    }
}

const mapStateToProps = state => {
    return {
        teachers: get(state, 'setTeacherSubjects.list.teachers'),
        teacherSubjects: get(state, 'setTeacherSubjects.list.teacherSubjects'),
        messageResult: get(state, 'setTeacherSubjects.list.messageResult'),
        errors: get(state, 'setTeacherSubjects.list.errors'),
        loading: get(state, 'setTeacherSubjects.list.loading'),
        failed: get(state, 'setTeacherSubjects.list.failed'),
        success: get(state, 'setTeacherSubjects.list.success')
    };
}

const mapDispatchToProps = (dispatch) => {
    return {
        changeTeacherSubjects: filter => {
            dispatch(changeTeachSubjListActions.changeTeacherSubjects(filter));
        },
        getTeacherSubjects: filter => {
            dispatch(getTeachSubjListActions.getTeacherSubjects(filter));
        },
        getTeachers: () => {
            dispatch(getTeachListActions.getTeachers());
        }
    }
}

export default connect(mapStateToProps, mapDispatchToProps)(SetTeacherSubjects);
