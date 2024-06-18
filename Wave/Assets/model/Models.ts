export type guid = string;

export enum CategoryColor {
    Primary = 1,
    Dangerous = 5,
    Important = 10,
    Informative = 15,
    Secondary = 20,
    Default = 25,
    Extra = 50,
}
export enum ArticleStatus {
    Draft = 0,
    InReview = 1,
    Published = 2,
}

export type Category = {
    id: guid,
    name: string,
    color: CategoryColor,
}

export type ArticleView = {
    id: guid,
    title: string,
    slug: string,
    body: string,
    status: ArticleStatus,
    publishDate: string,
    categories: Category[],
}
export type ArticleDto = {
    id: guid,
    title: string,
    slug: string,
    body: string,
    publishDate: Date,
    categories: guid[],
}