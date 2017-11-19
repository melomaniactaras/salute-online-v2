// system
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { AppComponent } from './components/app/app.component';
import { CanActivate } from '@angular/router';
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { HTTP_INTERCEPTORS } from '@angular/common/http';

// third party

import {
    MatButtonModule, MatIconModule, MatMenuModule, MatDialogModule, MatTabsModule, MatInputModule, MatFormFieldModule, MatSnackBarModule, MatCardModule, MatDatepickerModule,
    MatNativeDateModule, MatGridListModule, MatAutocompleteModule, MatExpansionModule, MatTableModule, MatPaginatorModule, MatSelectModule,
    MATERIAL_SANITY_CHECKS, MAT_DATE_LOCALE, MAT_NATIVE_DATE_FORMATS, MAT_DATE_FORMATS
} from "@angular/material";

import { TreeModule } from 'primeng/primeng';
import { MomentModule } from "angular2-moment";

// old

import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { HomeComponent } from './components/home/home.component';
import { FetchDataComponent } from './components/fetchdata/fetchdata.component';
import { CounterComponent } from './components/counter/counter.component';

// components

import { SoSidebar } from "./components/so-sidebar/so-sidebar.component";
import { SoHeader } from "./components/so-header/so-header.component";
import { LoginDialog } from "./components/so-login-dialog/so-login-dialog";
import { SoMenu } from "./components/so-menu/so-menu.component";
import { SoMenuItem } from "./components/so-menu-item/so-menu-item.component";
import { SoUserProfile } from "./components/so-user-profile/so-user-profile.component";
import { SoClubsList } from "./components/so-clubs-list/so-clubs-list.component";
import { CreateClubDialog } from "./components/so-create-club-dialog/so-create-club-dialog";
import { SoClubsEdit } from "./components/so-club-edit/so-club-edit";
import { AddClubMemberDialog } from "./components/so-add-club-member/so-add-club-member";
import { MembershipRequestDialog } from "./components/so-membership-request-dialog/so-membership-request-dialog";
import { SoMessages } from "./components/so-messages/so-messages";
import { SoProtocol } from "./components/so-protocol/so-protocol";

// services

import { AuthGuard } from "./services/authGuard";
import { GlobalState } from "./services/global.state";
import { AuthService } from "./services/auth";
import { Helpers } from "./services/helpers";
import { CustomDataSource } from "./services/datatable.service";
import { AuthInterceptor } from "./services/httpInterceptor";

// context


import { ClubsApi } from "./services/context/clubsApi";
import { CommonApi } from "./services/context/commonApi";
import { UserApi } from "./services/context/userApi";
import { InnerMessageApi } from "./services/context/innerMessageApi";
import { Context } from "./services/context/context";

// pipes

import { TruncatePipe } from "./pipes/string-pipes";

@
NgModule({
    declarations: [
        AppComponent,
        NavMenuComponent,
        CounterComponent,
        FetchDataComponent,
        HomeComponent,
        SoSidebar,
        LoginDialog,
        SoHeader,
        SoMenu,
        SoMenuItem,
        SoUserProfile,
        SoClubsList,
        CreateClubDialog,
        AddClubMemberDialog,
        MembershipRequestDialog,
        SoClubsEdit,
        SoMessages,
        TruncatePipe,
        SoProtocol
    ],
    entryComponents: [LoginDialog, CreateClubDialog, AddClubMemberDialog, MembershipRequestDialog],
    providers: [ 
        Helpers,
        CustomDataSource,
        GlobalState,
        AuthService,
        CommonApi,
        UserApi,
        ClubsApi,
        InnerMessageApi,
        Context,
        { provide: MAT_DATE_LOCALE, useValue: 'en-GB' },
        { provide: MAT_DATE_FORMATS, useValue: MAT_NATIVE_DATE_FORMATS },
        { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
    ],
    imports: [
        FormsModule,
        ReactiveFormsModule,
        MatMenuModule,
        MatIconModule,
        MatButtonModule,
        MatDialogModule,
        MatTabsModule,
        MatInputModule,
        MatFormFieldModule,
        MatSnackBarModule,
        MatCardModule,
        MatDatepickerModule,
        MatNativeDateModule,
        MatGridListModule,
        MatAutocompleteModule,
        MatExpansionModule,
        MatTableModule,
        MatPaginatorModule,
        MatSelectModule,
        CommonModule,
        HttpModule,
        FormsModule,
        BrowserAnimationsModule,
        TreeModule,
        MomentModule,
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent },
            { path: 'counter', component: CounterComponent },
            { path: 'fetch-data', component: FetchDataComponent },
            { path: 'so-user-profile', component: SoUserProfile },
            { path: 'so-clubs-list', component: SoClubsList },
            { path: 'so-club-edit/:id', component: SoClubsEdit },
            { path: 'so-messages', component: SoMessages },
            { path: 'so-protocol', component: SoProtocol },
            { path: 'so-protocol/:id', component: SoProtocol },
            { path: '**', redirectTo: 'home' }
        ])
    ]
})
export class AppModuleShared {
}
