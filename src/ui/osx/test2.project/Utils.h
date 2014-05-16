//
//  Utils.h
//  TogglDesktop
//
//  Created by Tanel Lebedev on 07/05/14.
//  Copyright (c) 2014 Toggl Desktop developers. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface Utils : NSObject

+ (NSInteger)boolToState:(BOOL)value;
+ (unsigned int)stateToBool:(NSInteger)state;
+ (void)disallowDuplicateInstances;
+ (NSString *)applicationSupportDirectory:(NSString *)environment;
+ (bool)isNumeric:(NSString *)checkText;

@end

BOOL wasLaunchedAsLoginOrResumeItem();
BOOL wasLaunchedAsHiddenLoginItem();
