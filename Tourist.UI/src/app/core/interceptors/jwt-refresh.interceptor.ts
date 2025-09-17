import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, switchMap, take } from 'rxjs/operators';
import { CookieService } from 'ngx-cookie-service';
import { LoginService } from '../../auth/login/services/login.service';
import { Router } from '@angular/router';

let isRefreshing = false;
const refreshTokenSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);

export const jwtRefreshInterceptorFn: HttpInterceptorFn = (req: HttpRequest<any>, next: HttpHandlerFn) => {
    const cookieService = inject(CookieService);
    const loginService = inject(LoginService);
    const router = inject(Router);
    const token = cookieService.get('Authorization');
    let authReq = req;
    if (token && token.startsWith('Bearer ')) {
        authReq = req.clone({
            setHeaders: { Authorization: token }
        });
    }
    return next(authReq).pipe(
        catchError((error: HttpErrorResponse) => {
            if (error.status === 401 && !authReq.url.endsWith('/login') && !authReq.url.endsWith('/refreshtoken')) {
                return handle401Error(authReq, next, cookieService, loginService, router);
            }
            return throwError(() => error);
        })
    );
};

function handle401Error(request: HttpRequest<any>, next: HttpHandlerFn, cookieService: CookieService, loginService: LoginService, router: Router): Observable<any> {
    if (!isRefreshing) {
        isRefreshing = true;
        refreshTokenSubject.next(null);
        const refreshToken = cookieService.get('RefreshToken');

        if (!refreshToken) {
            router.navigate(['/login']);
            return throwError(() => new Error('No refresh token'));
        }
        const email = localStorage.getItem('userEmail');

        return loginService.refreshToken(refreshToken, email).pipe(
            switchMap((res: any) => {
                isRefreshing = false;
                cookieService.set('Authorization', `Bearer ${res.token}`, undefined, '/', undefined, true, 'Strict');
                refreshTokenSubject.next(res.token);
                return next(request.clone({
                    setHeaders: { Authorization: `Bearer ${res.token}` }
                }));
            }),
            catchError((err) => {
                isRefreshing = false;
                router.navigate(['/login']);
                return throwError(() => err);
            })
        );
    } else {
        return refreshTokenSubject.pipe(
            filter(token => token != null),
            take(1),
            switchMap(token => next(request.clone({
                setHeaders: { Authorization: token! }
            })))
        );
    }
}
