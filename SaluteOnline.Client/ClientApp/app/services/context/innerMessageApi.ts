﻿import { Injector, Injectable } from "@angular/core";
import { HttpClient } from '@angular/common/http';
import { apiSettings } from "../../configuration/constants";
import { Observable } from 'rxjs';
import { InnerMessagesFilter, InnerMessageDto } from "../../dto/dto";

@Injectable()
export class InnerMessageApi {
    urls: any = {
        getMessages: 'innerMessages'
    }

    constructor(private readonly http: HttpClient) {
    }

    getMessages(filter: InnerMessagesFilter): Observable<InnerMessageDto[]> {
        return this.http.post<InnerMessageDto[]>(apiSettings.baseUrl + this.urls.getMessages, filter);
    }
}